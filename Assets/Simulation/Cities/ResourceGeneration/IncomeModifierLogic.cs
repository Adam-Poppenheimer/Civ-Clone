using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;
using Assets.Simulation.Improvements;

namespace Assets.Simulation.Cities.ResourceGeneration {

    /// <summary>
    /// The standard implementation of IIncomeModifierLogic.
    /// </summary>
    public class IncomeModifierLogic : IIncomeModifierLogic {

        #region instance fields and properties

        private IPossessionRelationship<ICity, IBuilding> BuildingPossessionCanon;

        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        private IHexGrid Map;

        #endregion

        #region constructors

        [Inject]
        public IncomeModifierLogic(
            IPossessionRelationship<ICity, IBuilding> buildingPossessionCanon,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IHexGrid map
        ){
            BuildingPossessionCanon  = buildingPossessionCanon;
            CityPossessionCanon      = cityPossessionCanon;
            Map                      = map;
        }

        #endregion

        #region instance methods

        #region from IIncomeModifierLogic

        /// <inheritdoc/>
        public ResourceSummary GetYieldMultipliersForCity(ICity city) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }

            var baseModifier = ResourceSummary.Empty;

            foreach(var building in BuildingPossessionCanon.GetPossessionsOfOwner(city)) {
                baseModifier += building.CityYieldModifier;
            }

            return  baseModifier;
        }

        /// <inheritdoc/>
        public ResourceSummary GetYieldMultipliersForCivilization(ICivilization civilization) {
            if(civilization == null) {
                throw new ArgumentNullException("civilization");
            }

            var baseModifier = ResourceSummary.Empty;            

            foreach(var city in CityPossessionCanon.GetPossessionsOfOwner(civilization)) {
                foreach(var building in BuildingPossessionCanon.GetPossessionsOfOwner(city)) {
                    baseModifier += building.CivilizationYieldModifier;
                }
            }

            return  baseModifier;
        }

        /// <inheritdoc/>
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
