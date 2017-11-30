using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Cities.Territory;
using Assets.Simulation.Cities.Growth;
using Assets.Simulation.Cities.ResourceGeneration;

namespace Assets.Simulation.Cities.Distribution {

    public class WorkerDistributionLogic : IWorkerDistributionLogic {

        #region instance fields and properties

        private IPopulationGrowthLogic GrowthLogic;

        private IResourceGenerationLogic GenerationLogic;

        private IBuildingPossessionCanon BuildingCanon;

        private ITilePossessionCanon TileCanon;

        #endregion

        #region constructors

        [Inject]
        public WorkerDistributionLogic(IPopulationGrowthLogic growthLogic, IResourceGenerationLogic generationLogic,
            IBuildingPossessionCanon buildingCanon, ITilePossessionCanon tileCanon) {

            GrowthLogic     = growthLogic;
            GenerationLogic = generationLogic;
            BuildingCanon   = buildingCanon;
            TileCanon       = tileCanon;
        }

        #endregion

        #region instance methods

        #region from IWorkerDistributionLogic

        public void DistributeWorkersIntoSlots(int workerCount, IEnumerable<IWorkerSlot> slots, ICity sourceCity,
            ResourceFocusType focus){  
            foreach(var slot in slots) {
                slot.IsOccupied = false;
            }

            switch(focus) {
                case ResourceFocusType.Food:       PerformFocusedDistribution(workerCount, slots, sourceCity, ResourceType.Food);       break;
                case ResourceFocusType.Gold:       PerformFocusedDistribution(workerCount, slots, sourceCity, ResourceType.Gold);       break;
                case ResourceFocusType.Production: PerformFocusedDistribution(workerCount, slots, sourceCity, ResourceType.Production); break;
                case ResourceFocusType.Culture:    PerformFocusedDistribution(workerCount, slots, sourceCity, ResourceType.Culture);    break;
                case ResourceFocusType.TotalYield: PerformUnfocusedDistribution(workerCount, slots, sourceCity); break;
                default: break;
            } 
        }

        public int GetUnemployedPeopleInCity(ICity city) {
            int occupiedTiles = TileCanon.GetTilesOfCity(city).Where(tile => tile.WorkerSlot.IsOccupied).Count();

            int occupiedBuildingSlots = 0;
            foreach(var building in BuildingCanon.GetBuildingsInCity(city)) {
                occupiedBuildingSlots += building.Slots.Where(slot => slot.IsOccupied).Count();
            }

            int unemployedPeople = city.Population - (occupiedTiles + occupiedBuildingSlots);
            if(unemployedPeople < 0) {
                throw new NegativeUnemploymentException("This city has more occupied slots than it has people");
            }
            return unemployedPeople;
        }

        public IEnumerable<IWorkerSlot> GetSlotsAvailableToCity(ICity city) {
            var retval = new List<IWorkerSlot>();

            retval.AddRange(TileCanon.GetTilesOfCity(city).Where(tile => !tile.SuppressSlot).Select(tile => tile.WorkerSlot));

            foreach(var building in BuildingCanon.GetBuildingsInCity(city)) {
                retval.AddRange(building.Slots);
            }

            return retval;
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
            ResourceType focusedResource) {
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
