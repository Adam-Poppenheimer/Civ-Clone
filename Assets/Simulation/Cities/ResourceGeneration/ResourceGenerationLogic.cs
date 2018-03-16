using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.HexMap;
using Assets.Simulation.WorkerSlots;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Cities.ResourceGeneration {

    /// <summary>
    /// The standard implementation of IResourceGenerationLogic.
    /// </summary>
    public class ResourceGenerationLogic : IResourceGenerationLogic {

        #region instance fields and properties

        private ICityConfig                                   Config;
        private IPossessionRelationship<ICity, IHexCell>      CellPossessionCanon;
        private IPossessionRelationship<ICity, IBuilding>     BuildingCanon;
        private IIncomeModifierLogic                          IncomeModifierLogic;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private IPossessionRelationship<IHexCell, ICity>      CityLocationCanon;

        #endregion

        #region constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="cellPossessionCanon"></param>
        /// <param name="buildingCanon"></param>
        /// <param name="incomeModifierLogic"></param>
        /// <param name="cityPossessionCanon"></param>
        [Inject]
        public ResourceGenerationLogic(
            ICityConfig config, IPossessionRelationship<ICity, IHexCell> cellPossessionCanon,
            IPossessionRelationship<ICity, IBuilding> buildingCanon, IIncomeModifierLogic incomeModifierLogic,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon
        ){
            Config              = config;
            CellPossessionCanon = cellPossessionCanon;
            BuildingCanon       = buildingCanon;
            IncomeModifierLogic = incomeModifierLogic;
            CityPossessionCanon = cityPossessionCanon;
            CityLocationCanon   = cityLocationCanon;
        }

        #endregion

        #region instance methods

        #region from IResourceGenerationLogic

        /// <inheritdoc/>
        public ResourceSummary GetTotalYieldForCity(ICity city) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }

            var cityMultipliers = IncomeModifierLogic.GetYieldMultipliersForCity(city);
            var civMultipliers  = IncomeModifierLogic.GetYieldMultipliersForCivilization(CityPossessionCanon.GetOwnerOfPossession(city));

            var cityLocation = CityLocationCanon.GetOwnerOfPossession(city);

            var retval = cityLocation.WorkerSlot.BaseYield * (ResourceSummary.Ones + cityMultipliers + civMultipliers);

            int employedPops = 0;

            foreach(var tile in CellPossessionCanon.GetPossessionsOfOwner(city)) {
                if(tile.SuppressSlot || !tile.WorkerSlot.IsOccupied) {
                    continue;
                }

                retval += GetYieldOfSlotForCity(tile.WorkerSlot, city);
                if(tile.WorkerSlot.IsOccupied) {
                    employedPops++;
                }
            }

            foreach(var building in BuildingCanon.GetPossessionsOfOwner(city)) {
                foreach(var slot in building.Slots) {
                    if(!slot.IsOccupied) {
                        continue;
                    }

                    retval += GetYieldOfSlotForCity(slot, city);
                    employedPops++;
                }

                retval += GetNonSlotYieldOfBuildingForCity(building, city, cityMultipliers, civMultipliers);
            }

            retval += GetYieldOfUnemployedForCity(city) * Math.Max(0, city.Population - employedPops);

            retval[ResourceType.Science] += GetScienceFromPopulation(
                city, cityMultipliers[ResourceType.Science], civMultipliers[ResourceType.Science]
            );

            return retval;
        }

        /// <inheritdoc/>
        public ResourceSummary GetYieldOfSlotForCity(IWorkerSlot slot, ICity city) {
            if(slot == null) {
                throw new ArgumentNullException("slot");
            }else if(city == null) {
                throw new ArgumentNullException("city");
            }

            var multiplier = ResourceSummary.Ones +
                IncomeModifierLogic.GetYieldMultipliersForSlot(slot) +
                IncomeModifierLogic.GetYieldMultipliersForCity(city);   
                
            var owningCivilization = CityPossessionCanon.GetOwnerOfPossession(city);
            if(owningCivilization != null) {
                multiplier += IncomeModifierLogic.GetYieldMultipliersForCivilization(owningCivilization);
            }              

            return slot.BaseYield * multiplier;
        }

        /// <inheritdoc/>
        public ResourceSummary GetYieldOfUnemployedForCity(ICity city) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }

            var multiplier = ResourceSummary.Ones + IncomeModifierLogic.GetYieldMultipliersForCity(city);

            var owningCivilization = CityPossessionCanon.GetOwnerOfPossession(city);
            if(owningCivilization != null) {
                multiplier += IncomeModifierLogic.GetYieldMultipliersForCivilization(owningCivilization);
            }

            return Config.UnemployedYield * multiplier;
        }

        #endregion

        private ResourceSummary GetNonSlotYieldOfBuildingForCity(
            IBuilding building, ICity city, ResourceSummary cityMultipliers,
            ResourceSummary civMultipliers
        ){
            var retval = building.StaticYield;

            retval += building.Template.BonusYieldPerPopulation * city.Population;

            var multipliers = ResourceSummary.Ones + cityMultipliers + civMultipliers;

            return retval * multipliers;
        }

        private float GetScienceFromPopulation(ICity city, float cityMultiplier, float civMultiplier) {
            return city.Population * (1f + cityMultiplier + civMultiplier);
        }

        #endregion

    }

}
