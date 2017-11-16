using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.GameMap;

namespace Assets.Cities {

    public class WorkerDistributionLogic : IWorkerDistributionLogic {

        #region instance fields and properties

        private IPopulationGrowthLogic GrowthLogic;

        private IResourceGenerationLogic GenerationLogic;

        #endregion

        #region constructors

        [Inject]
        public WorkerDistributionLogic(IPopulationGrowthLogic growthLogic, IResourceGenerationLogic generationLogic) {
            GrowthLogic = growthLogic;
            GenerationLogic = generationLogic;
        }

        #endregion

        #region instance methods

        #region from IWorkerDistributionLogic

        public void DistributeWorkersIntoSlots(int workerCount, IEnumerable<IWorkerSlot> slots, ICity sourceCity,
            DistributionPreferences preferences){  
            foreach(var slot in slots) {
                slot.IsOccupied = false;
            }

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
        private void PerformFocusedDistribution(int workerCount, IEnumerable<IWorkerSlot> slots, ICity sourceCity,
            DistributionPreferences preferences) {
            var focusedResource = preferences.FocusedResource;

            var maximizingComparison = SlotComparisonUtil.BuildFocusedComparisonAscending(sourceCity, focusedResource, GenerationLogic);

            MaximizeYield(workerCount, slots, sourceCity, maximizingComparison);
            MitigateStarvation(workerCount, slots, sourceCity, maximizingComparison);
        }

        private void PerformUnfocusedDistribution(int workerCount, IEnumerable<IWorkerSlot> slots, ICity sourceCity) {
            var maximizingComparison = SlotComparisonUtil.BuildTotalYieldComparisonAscending(sourceCity, GenerationLogic);

            MaximizeYield(workerCount, slots, sourceCity, maximizingComparison);
            MitigateStarvation(workerCount, slots, sourceCity, maximizingComparison);
        }

        private void MaximizeYield(int workerCount, IEnumerable<IWorkerSlot> slots, ICity sourceCity,
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

        private void MitigateStarvation(int workerCount, IEnumerable<IWorkerSlot> slots, ICity sourceCity,
            Comparison<IWorkerSlot> yieldMaximizationComparer) {

            int foodProduced = GenerationLogic.GetTotalYieldForCity(sourceCity)[ResourceType.Food];
            int foodRequired = GrowthLogic.GetFoodConsumptionPerTurn(sourceCity);

            var occupiedByMaxYieldDescending = new List<IWorkerSlot>(slots.Where(slot => slot.IsOccupied));

            occupiedByMaxYieldDescending.Sort(yieldMaximizationComparer);
            occupiedByMaxYieldDescending.Reverse();

            var slotsByFoodThenFocusedYield = new List<IWorkerSlot>(slots);
            var foodComparer = SlotComparisonUtil.BuildResourceComparisonAscending(ResourceType.Food, sourceCity, GenerationLogic);

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

        #endregion
        
    }

}
