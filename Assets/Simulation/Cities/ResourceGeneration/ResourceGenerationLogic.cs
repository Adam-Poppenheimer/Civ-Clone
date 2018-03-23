using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.HexMap;
using Assets.Simulation.Cities.Distribution;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Cities.ResourceGeneration {

    /// <summary>
    /// The standard implementation of IResourceGenerationLogic.
    /// </summary>
    public class ResourceGenerationLogic : IResourceGenerationLogic {

        #region instance fields and properties

        private ICityConfig                                   Config;
        private IPossessionRelationship<ICity, IHexCell>      CellPossessionCanon;
        private IPossessionRelationship<ICity, IBuilding>     BuildingPossessionCanon;
        private IIncomeModifierLogic                          IncomeModifierLogic;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private ICellResourceLogic                            CellResourceLogic;
        private IBuildingResourceLogic                        BuildingResourceLogic;
        private IUnemploymentLogic                            UnemploymentLogic;

        #endregion

        #region constructors

        [Inject]
        public ResourceGenerationLogic(
            ICityConfig config, IPossessionRelationship<ICity, IHexCell> cellPossessionCanon,
            IPossessionRelationship<ICity, IBuilding> buildingPossessionCanon, IIncomeModifierLogic incomeModifierLogic,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            ICellResourceLogic cellResourceLogic, IBuildingResourceLogic buildingResourceLogic,
            IUnemploymentLogic unemploymentLogic
        ){
            Config                  = config;
            CellPossessionCanon     = cellPossessionCanon;
            BuildingPossessionCanon = buildingPossessionCanon;
            IncomeModifierLogic     = incomeModifierLogic;
            CityPossessionCanon     = cityPossessionCanon;
            CellResourceLogic       = cellResourceLogic;
            BuildingResourceLogic   = buildingResourceLogic;
            UnemploymentLogic       = unemploymentLogic;
        }

        #endregion

        #region instance methods

        #region from IResourceGenerationLogic

        /// <inheritdoc/>
        public ResourceSummary GetTotalYieldForCity(ICity city) {
            return GetTotalYieldForCity(city, ResourceSummary.Empty);
        } 

        public ResourceSummary GetTotalYieldForCity(ICity city, ResourceSummary additionalBonuses) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }

            var retval = GetBaseYieldOfCity(city);

            foreach(var cell in CellPossessionCanon.GetPossessionsOfOwner(city)) {
                if(!cell.SuppressSlot && cell.WorkerSlot.IsOccupied) {
                    retval += GetYieldOfCellForCity(cell, city);
                }                
            }

            foreach(var building in BuildingPossessionCanon.GetPossessionsOfOwner(city)) {
                retval += GetYieldOfBuildingForCity(building, city);
            }

            retval += GetYieldOfUnemployedPersonForCity(city) * UnemploymentLogic.GetUnemployedPeopleInCity(city);

            return retval;
        }

        public ResourceSummary GetBaseYieldOfCity(ICity city) {
            var cityMultipliers = IncomeModifierLogic.GetYieldMultipliersForCity(city);
            var civMultipliers  = IncomeModifierLogic.GetYieldMultipliersForCivilization(CityPossessionCanon.GetOwnerOfPossession(city));

            var retval = Config.LocationYield * (ResourceSummary.Ones + cityMultipliers + civMultipliers);

            retval += new ResourceSummary(science: 1) * city.Population * (ResourceSummary.Ones + cityMultipliers + civMultipliers);

            return retval;
        }

        /// <inheritdoc/>
        public ResourceSummary GetYieldOfUnemployedPersonForCity(ICity city) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }

            return Config.UnemployedYield * GetMultiplier(city);
        }

        public ResourceSummary GetYieldOfCellForCity(IHexCell cell, ICity city) {
            if(cell == null) {
                throw new ArgumentNullException("slot");
            }else if(city == null) {
                throw new ArgumentNullException("city");
            }            

            return CellResourceLogic.GetYieldOfCell(cell) * GetMultiplier(city);
        }

        public ResourceSummary GetYieldOfBuildingForCity(IBuilding building, ICity city) {
            var multiplier = GetMultiplier(city);

            var buildingYield = BuildingResourceLogic.GetYieldOfBuilding(building) * multiplier;

            return buildingYield;
        }

        public ResourceSummary GetYieldOfBuildingSlotsForCity(IBuilding building, ICity city) {
            var multiplier = GetMultiplier(city);

            var slotYield = building.Template.SlotYield * multiplier;

            return slotYield;
        }

        #endregion

        private ResourceSummary GetMultiplier(ICity city) {
            var owningCivilization = CityPossessionCanon.GetOwnerOfPossession(city);

            return ResourceSummary.Ones + IncomeModifierLogic.GetYieldMultipliersForCity(city)
                + IncomeModifierLogic.GetYieldMultipliersForCivilization(owningCivilization);
        }

        #endregion

    }

}
