using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Cities.ResourceGeneration {

    public class IncomeModifierLogic : IIncomeModifierLogic {

        #region instance fields and properties

        private IBuildingPossessionCanon BuildingPossessionCanon;

        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public IncomeModifierLogic(IBuildingPossessionCanon buildingPossessionCanon,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon) {

            BuildingPossessionCanon = buildingPossessionCanon;
            CityPossessionCanon = cityPossessionCanon;
        }

        #endregion

        #region instance methods

        #region from IIncomeModifierLogic

        public ResourceSummary GetYieldMultipliersForCity(ICity city) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }

            var baseModifier = ResourceSummary.Empty;

            foreach(var building in BuildingPossessionCanon.GetBuildingsInCity(city)) {
                baseModifier += building.Template.CityYieldModifier;
            }

            return  baseModifier;
        }

        public ResourceSummary GetYieldMultipliersForCivilization(ICivilization civilization) {
            if(civilization == null) {
                throw new ArgumentNullException("civilization");
            }

            var baseModifier = ResourceSummary.Empty;            

            foreach(var city in CityPossessionCanon.GetPossessionsOfOwner(civilization)) {
                foreach(var building in BuildingPossessionCanon.GetBuildingsInCity(city)) {
                    baseModifier += building.Template.CivilizationYieldModifier;
                }
            }

            return  baseModifier;
        }

        public ResourceSummary GetYieldMultipliersForSlot(IWorkerSlot slot) {
            if(slot == null) {
                throw new ArgumentNullException("slot");
            }

            return ResourceSummary.Empty;
        }

        #endregion

        #endregion
        
    }

}
