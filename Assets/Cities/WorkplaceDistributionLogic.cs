using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.GameMap;

namespace Assets.Cities {

    public class WorkplaceDistributionLogic : IWorkerDistributionLogic {

        #region instance fields and properties

        private IPopulationGrowthLogic GrowthLogic;

        private IResourceGenerationLogic GenerationLogic;

        #endregion

        #region constructors

        [Inject]
        public WorkplaceDistributionLogic(IPopulationGrowthLogic growthLogic, IResourceGenerationLogic generationLogic) {
            GrowthLogic = growthLogic;
            GenerationLogic = generationLogic;
        }

        #endregion

        #region instance methods

        #region from IWorkerDistributionLogic

        public void DistributeWorkersIntoSlots(int workerCount, List<IWorkerSlot> slots, ICity sourceCity,
            DistributionPreferences preferences){            
            slots.ForEach(slot => slot.IsOccupied = false);

            if(preferences.ShouldFocusResource) {
                PerformFocusedDistribution(workerCount, slots, sourceCity, preferences);                
            }else {
                PerformUnfocusedDistribution(workerCount, slots, sourceCity);                
            }  
        }

        #endregion

        /// <remarks>
        /// This algorithm works in two stages. First, it assigns workers to the highest
        /// possible yield for the resource focus preferences is requesting. Once that's been
        /// done, it checks to see if its current distribution is capable of sustaining the
        /// population. If it isn't, it starts removing workers from the slots that yield
        /// the least of the focused resource and starts placing them in the slots that yield
        /// the most food. It does this until the total food yield is sufficient to sustain
        /// the population.
        /// </remarks>
        private void PerformFocusedDistribution(int workerCount, List<IWorkerSlot> slots, ICity sourceCity,
            DistributionPreferences preferences) {
            var focusedResource = preferences.FocusedResource;

            var maximizingComparison = BuildFocusedMaximizingComparison(sourceCity, focusedResource);

            MaximizeYield(workerCount, slots, sourceCity, maximizingComparison);
            MitigateStarvation(workerCount, slots, sourceCity, maximizingComparison);
        }

        private void PerformUnfocusedDistribution(int workerCount, List<IWorkerSlot> slots, ICity sourceCity) {
            var maximizingComparison = BuildUnfocusedMaximizingComparison(sourceCity);

            MaximizeYield(workerCount, slots, sourceCity, maximizingComparison);
            MitigateStarvation(workerCount, slots, sourceCity, maximizingComparison);
        }

        private void MaximizeYield(int workerCount, List<IWorkerSlot> slots, ICity sourceCity,
            Comparison<IWorkerSlot> yieldMaximizationComparer){

            var maximizedSlotsAscending = new List<IWorkerSlot>(slots);

            maximizedSlotsAscending.Sort(yieldMaximizationComparer);

            var occupiedSlots = new List<IWorkerSlot>();

            int employedWorkers = 0;
            for(; employedWorkers < workerCount; ++employedWorkers) {
                var nextBestSlot = maximizedSlotsAscending.LastOrDefault();
                if(nextBestSlot != null) {
                    nextBestSlot.IsOccupied = true;
                    maximizedSlotsAscending.Remove(nextBestSlot);
                    occupiedSlots.Add(nextBestSlot);
                }else {
                    return;
                }
            }
        }

        private void MitigateStarvation(int workerCount, List<IWorkerSlot> slots, ICity sourceCity,
            Comparison<IWorkerSlot> yieldMaximizationComparer) {

            int foodProduced = GenerationLogic.GetTotalYieldForCity(sourceCity)[ResourceType.Food];
            int foodRequired = GrowthLogic.GetFoodConsumptionPerTurn(sourceCity);

            var occupiedByMaxYieldDescending = new List<IWorkerSlot>(slots.Where(slot => slot.IsOccupied));

            occupiedByMaxYieldDescending.Sort(yieldMaximizationComparer);
            occupiedByMaxYieldDescending.Reverse();

            var slotsByFoodThenFocusedYield = new List<IWorkerSlot>(slots);
            var foodComparer = BuildResourceComparisonAscending(ResourceType.Food, sourceCity);

            slotsByFoodThenFocusedYield.Sort(delegate(IWorkerSlot firstSlot, IWorkerSlot secondSlot) {
                int foodComparison = foodComparer(firstSlot, secondSlot);

                return foodComparison != 0 ? foodComparison : yieldMaximizationComparer(firstSlot, secondSlot);
            });

            while(foodProduced < foodRequired) {
                var worstOccupiedForFocus = occupiedByMaxYieldDescending.LastOrDefault();
                var bestUnoccupiedForFood = slotsByFoodThenFocusedYield.LastOrDefault(slot => !slot.IsOccupied);

                if(worstOccupiedForFocus != null && bestUnoccupiedForFood != null) {
                    worstOccupiedForFocus.IsOccupied = false;
                    bestUnoccupiedForFood.IsOccupied = true;

                    occupiedByMaxYieldDescending.Remove(worstOccupiedForFocus);
                    slotsByFoodThenFocusedYield.Remove(bestUnoccupiedForFood);

                    foodProduced = GenerationLogic.GetTotalYieldForCity(sourceCity)[ResourceType.Food];
                }else {
                    break;
                }
            }
        }

        private Comparison<IWorkerSlot> BuildFocusedMaximizingComparison(ICity sourceCity, ResourceType focusedResource) {
            return delegate(IWorkerSlot firstSlot, IWorkerSlot secondSlot) {
                var firstYield = GenerationLogic.GetYieldOfSlotForCity(firstSlot, sourceCity);
                var secondYield = GenerationLogic.GetYieldOfSlotForCity(secondSlot, sourceCity);

                var focusComparison = firstYield[focusedResource].CompareTo(secondYield[focusedResource]);
                if(focusComparison == 0) {
                    focusComparison = firstYield[ResourceType.Food].CompareTo(secondYield[ResourceType.Food]);
                }
                if(focusComparison == 0) {
                    focusComparison = firstYield.Total.CompareTo(secondYield.Total);
                }

                return focusComparison;
            };
        }

        private Comparison<IWorkerSlot> BuildResourceComparisonAscending(ResourceType focusedResource, ICity sourceCity) {
            return delegate(IWorkerSlot firstSlot, IWorkerSlot secondSlot) {
                var firstYield = GenerationLogic.GetYieldOfSlotForCity(firstSlot, sourceCity);
                var secondYield = GenerationLogic.GetYieldOfSlotForCity(secondSlot, sourceCity);

                return firstYield[focusedResource].CompareTo(secondYield[focusedResource]);
            };
        }

        private Comparison<IWorkerSlot> BuildUnfocusedMaximizingComparison(ICity sourceCity) {
            return delegate(IWorkerSlot firstSlot, IWorkerSlot secondSlot) {
                var firstYield = GenerationLogic.GetYieldOfSlotForCity(firstSlot, sourceCity);
                var secondYield = GenerationLogic.GetYieldOfSlotForCity(secondSlot, sourceCity);

                return firstYield.Total.CompareTo(secondYield.Total);
            };
        }

        #endregion
        
    }

}
