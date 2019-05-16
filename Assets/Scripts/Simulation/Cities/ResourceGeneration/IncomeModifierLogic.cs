using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;
using Assets.Simulation.WorkerSlots;

namespace Assets.Simulation.Cities.ResourceGeneration {

    /// <summary>
    /// The standard implementation of IIncomeModifierLogic.
    /// </summary>
    public class IncomeModifierLogic : IIncomeModifierLogic {

        #region instance fields and properties

        private IPossessionRelationship<ICity, IBuilding>     BuildingPossessionCanon;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private ICivilizationHappinessLogic                   CivHappinessLogic;
        private ICivilizationConfig                           CivConfig;
        private IGoldenAgeCanon                               GoldenAgeCanon;

        #endregion

        #region constructors

        [Inject]
        public IncomeModifierLogic(
            IPossessionRelationship<ICity, IBuilding> buildingPossessionCanon,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            ICivilizationHappinessLogic civHappinessLogic,
            ICivilizationConfig civConfig, IGoldenAgeCanon goldenAgeCanon
        ){
            BuildingPossessionCanon = buildingPossessionCanon;
            CityPossessionCanon     = cityPossessionCanon;
            CivHappinessLogic       = civHappinessLogic;
            CivConfig               = civConfig;
            GoldenAgeCanon          = goldenAgeCanon;
        }

        #endregion

        #region instance methods

        #region from IIncomeModifierLogic

        /// <inheritdoc/>
        public YieldSummary GetYieldMultipliersForCity(ICity city) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }

            var baseModifier = YieldSummary.Empty;

            foreach(var building in BuildingPossessionCanon.GetPossessionsOfOwner(city)) {
                baseModifier += building.CityYieldModifier;
            }

            return  baseModifier;
        }

        /// <inheritdoc/>
        public YieldSummary GetYieldMultipliersForCivilization(ICivilization civ) {
            if(civ == null) {
                throw new ArgumentNullException("civilization");
            }

            var retval = YieldSummary.Empty;            

            foreach(var city in CityPossessionCanon.GetPossessionsOfOwner(civ)) {
                foreach(var building in BuildingPossessionCanon.GetPossessionsOfOwner(city)) {
                    retval += building.CivilizationYieldModifier;
                }
            }

            int civHappiness = CivHappinessLogic.GetNetHappinessOfCiv(civ);

            if(civHappiness < 0) {
                var goldAndProductionLoss = civHappiness * CivConfig.YieldLossPerUnhappiness;

                retval[YieldType.Gold]       += goldAndProductionLoss;
                retval[YieldType.Production] += goldAndProductionLoss;
            }

            if(GoldenAgeCanon.IsCivInGoldenAge(civ)) {
                retval += CivConfig.GoldenAgeProductionModifiers;
            }

            return  retval;
        }

        /// <inheritdoc/>
        public YieldSummary GetYieldMultipliersForSlot(IWorkerSlot slot) {
            if(slot == null) {
                throw new ArgumentNullException("slot");
            }

            return YieldSummary.Empty;
        }

        #endregion

        #endregion
        
    }

}
