using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.HexMap;
using Assets.Simulation.Cities.Growth;
using Assets.Simulation.Cities.ResourceGeneration;
using Assets.Simulation.WorkerSlots;

namespace Assets.Simulation.Cities.Distribution {

    /// <summary>
    /// The standard implementation of IWorkerDistributionLogic.
    /// </summary>
    public class WorkerDistributionLogic : IWorkerDistributionLogic {

        #region instance fields and properties

        private IPopulationGrowthLogic GrowthLogic;

        private IYieldGenerationLogic GenerationLogic;

        private IPossessionRelationship<ICity, IBuilding> BuildingPossessionCanon;

        private IPossessionRelationship<ICity, IHexCell> CellPossessionCanon;

        #endregion

        #region constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="growthLogic"></param>
        /// <param name="generationLogic"></param>
        /// <param name="buildingCanon"></param>
        /// <param name="tileCanon"></param>
        [Inject]
        public WorkerDistributionLogic(
            IPopulationGrowthLogic growthLogic, IYieldGenerationLogic generationLogic,
            IPossessionRelationship<ICity, IBuilding> buildingCanon, IPossessionRelationship<ICity, IHexCell> tileCanon
        ){
            GrowthLogic     = growthLogic;
            GenerationLogic = generationLogic;
            BuildingPossessionCanon   = buildingCanon;
            CellPossessionCanon       = tileCanon;
        }

        #endregion

        #region instance methods

        #region from IWorkerDistributionLogic

        /// <inheritdoc/>
        public void DistributeWorkersIntoSlots(
            int workerCount, IEnumerable<IWorkerSlot> slots, ICity sourceCity,
            YieldFocusType focus
        ){
            foreach(var slot in slots) {
                slot.IsOccupied = false;
            }

            switch(focus) {
                case YieldFocusType.Food:       PerformFocusedDistribution(workerCount, slots, sourceCity, YieldType.Food);       break;
                case YieldFocusType.Gold:       PerformFocusedDistribution(workerCount, slots, sourceCity, YieldType.Gold);       break;
                case YieldFocusType.Production: PerformFocusedDistribution(workerCount, slots, sourceCity, YieldType.Production); break;
                case YieldFocusType.Culture:    PerformFocusedDistribution(workerCount, slots, sourceCity, YieldType.Culture);    break;
                case YieldFocusType.Science:    PerformFocusedDistribution(workerCount, slots, sourceCity, YieldType.Science);    break;
                case YieldFocusType.TotalYield: PerformUnfocusedDistribution(workerCount, slots, sourceCity); break;
                default: break;
            } 
        }

        /// <inheritdoc/>
        public IEnumerable<IWorkerSlot> GetSlotsAvailableToCity(ICity city) {
            var retval = new List<IWorkerSlot>();

            retval.AddRange(CellPossessionCanon.GetPossessionsOfOwner(city).Where(tile => !tile.SuppressSlot).Select(tile => tile.WorkerSlot));

            foreach(var building in BuildingPossessionCanon.GetPossessionsOfOwner(city)) {
                retval.AddRange(building.Slots);
            }

            return retval;
        }

        #endregion

        /// <remarks>
        /// This algorithm works in two stages. First, it assigns workers based purely on the 
        /// given focused resource. Once that's been done, it checks to see if its current
        /// distribution is capable of sustaining the population. If it isn't, it starts
        /// removing workers from the slots that yield the least of the focused resource and
        /// places them in the slots that yield the most food. It does this until the total food
        /// yield is sufficient to sustain the population.
        /// </remarks>
        private void PerformFocusedDistribution(int workerCount, IEnumerable<IWorkerSlot> slots, ICity sourceCity,
            YieldType focusedResource) {
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

            int foodProduced = Mathf.FloorToInt(GenerationLogic.GetTotalYieldForCity(sourceCity)[YieldType.Food]);
            int foodRequired = GrowthLogic.GetFoodConsumptionPerTurn(sourceCity);

            var occupiedByMaxYieldDescending = new List<IWorkerSlot>(slots.Where(slot => slot.IsOccupied));

            occupiedByMaxYieldDescending.Sort(yieldMaximizationComparer);
            occupiedByMaxYieldDescending.Reverse();

            var slotsByFoodThenFocusedYield = new List<IWorkerSlot>(slots);
            var foodComparer = SlotComparisonUtil.BuildYieldComparisonAscending(YieldType.Food, sourceCity, GenerationLogic);

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

                    foodProduced = Mathf.FloorToInt(GenerationLogic.GetTotalYieldForCity(sourceCity)[YieldType.Food]);
                }else {
                    break;
                }
            }
        }

        #endregion
        
    }

}
