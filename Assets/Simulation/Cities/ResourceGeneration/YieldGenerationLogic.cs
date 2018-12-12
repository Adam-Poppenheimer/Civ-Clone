using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Cities.ResourceGeneration {

    /// <summary>
    /// The standard implementation of IResourceGenerationLogic.
    /// </summary>
    public class YieldGenerationLogic : IYieldGenerationLogic {

        #region instance fields and properties

        private ICityConfig                                   Config;
        private IPossessionRelationship<ICity, IHexCell>      CellPossessionCanon;
        private IPossessionRelationship<ICity, IBuilding>     BuildingPossessionCanon;
        private IIncomeModifierLogic                          IncomeModifierLogic;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private ICellYieldLogic                               CellYieldLogic;
        private IBuildingInherentYieldLogic                   BuildingYieldLogic;
        private IUnemploymentLogic                            UnemploymentLogic;
        private ICityCenterYieldLogic                         CityCenterYieldLogic;

        #endregion

        #region constructors

        [Inject]
        public YieldGenerationLogic(
            ICityConfig                                   config,
            IPossessionRelationship<ICity, IHexCell>      cellPossessionCanon,
            IPossessionRelationship<ICity, IBuilding>     buildingPossessionCanon,
            IIncomeModifierLogic                          incomeModifierLogic,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            ICellYieldLogic                               cellResourceLogic,
            IBuildingInherentYieldLogic                   buildingResourceLogic,
            IUnemploymentLogic                            unemploymentLogic,
            ICityCenterYieldLogic                         cityCenterYieldLogic
        ){
            Config                  = config;
            CellPossessionCanon     = cellPossessionCanon;
            BuildingPossessionCanon = buildingPossessionCanon;
            IncomeModifierLogic     = incomeModifierLogic;
            CityPossessionCanon     = cityPossessionCanon;
            CellYieldLogic          = cellResourceLogic;
            BuildingYieldLogic      = buildingResourceLogic;
            UnemploymentLogic       = unemploymentLogic;
            CityCenterYieldLogic    = cityCenterYieldLogic;
        }

        #endregion

        #region instance methods

        #region from IResourceGenerationLogic

        /// <inheritdoc/>
        public YieldSummary GetTotalYieldForCity(ICity city) {
            return GetTotalYieldForCity(city, YieldSummary.Empty);
        } 

        public YieldSummary GetTotalYieldForCity(ICity city, YieldSummary additionalBonuses) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }

            var retval = CityCenterYieldLogic.GetYieldOfCityCenter(city);

            foreach(var cell in CellPossessionCanon.GetPossessionsOfOwner(city)) {
                if(!cell.SuppressSlot && cell.WorkerSlot.IsOccupied) {
                    retval += GetYieldOfCellForCity(cell, city);
                }                
            }

            foreach(var building in BuildingPossessionCanon.GetPossessionsOfOwner(city)) {
                retval += GetYieldOfBuildingForCity(building, city);
            }

            retval += GetYieldOfUnemployedPersonForCity(city) * UnemploymentLogic.GetUnemployedPeopleInCity(city);

            return retval * (YieldSummary.Ones + additionalBonuses);
        }

        /// <inheritdoc/>
        public YieldSummary GetYieldOfUnemployedPersonForCity(ICity city) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }

            return Config.UnemployedYield * GetMultiplier(city);
        }

        public YieldSummary GetYieldOfCellForCity(IHexCell cell, ICity city) {
            if(cell == null) {
                throw new ArgumentNullException("slot");
            }else if(city == null) {
                throw new ArgumentNullException("city");
            }

            ICivilization owner = CityPossessionCanon.GetOwnerOfPossession(city);

            return CellYieldLogic.GetYieldOfCell(cell, owner) * GetMultiplier(city);
        }

        public YieldSummary GetYieldOfBuildingForCity(IBuilding building, ICity city) {
            var multiplier = GetMultiplier(city);

            var buildingYield = BuildingYieldLogic.GetYieldOfBuilding(building) * multiplier;

            return buildingYield;
        }

        public YieldSummary GetYieldOfBuildingSlotsForCity(IBuilding building, ICity city) {
            var multiplier = GetMultiplier(city);

            var slotYield = building.Template.Specialist.Yield * multiplier;

            return slotYield;
        }

        #endregion

        private YieldSummary GetMultiplier(ICity city) {
            var owningCivilization = CityPossessionCanon.GetOwnerOfPossession(city);

            return YieldSummary.Ones + IncomeModifierLogic.GetYieldMultipliersForCity(city)
                + IncomeModifierLogic.GetYieldMultipliersForCivilization(owningCivilization);
        }

        #endregion

    }

}
