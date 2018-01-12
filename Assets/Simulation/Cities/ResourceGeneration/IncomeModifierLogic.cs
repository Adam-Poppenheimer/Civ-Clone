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

        private IBuildingPossessionCanon BuildingPossessionCanon;

        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        private IHexGrid Map;

        private IImprovementLocationCanon ImprovementLocationCanon;

        #endregion

        #region constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buildingPossessionCanon"></param>
        /// <param name="cityPossessionCanon"></param>
        /// <param name="improvementLocationCanon"></param>
        /// <param name="map"></param>
        [Inject]
        public IncomeModifierLogic(
            IBuildingPossessionCanon buildingPossessionCanon,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IImprovementLocationCanon improvementLocationCanon,
            IHexGrid map
        ){
            BuildingPossessionCanon  = buildingPossessionCanon;
            CityPossessionCanon      = cityPossessionCanon;
            ImprovementLocationCanon = improvementLocationCanon;
            Map                      = map;
        }

        #endregion

        #region instance methods

        #region from IIncomeModifierLogic

        /// <inheritdoc/>
        public ResourceSummary GetRealBaseYieldForSlot(IWorkerSlot slot) {
            if(slot == null) {
                throw new ArgumentNullException("slot");
            }

            var baseYield = slot.BaseYield;

            var tileOfSlot = Map.Tiles.Where(tile => tile.WorkerSlot == slot).FirstOrDefault();

            if(tileOfSlot != null) {
                var improvementOnTile = ImprovementLocationCanon.GetPossessionsOfOwner(tileOfSlot).FirstOrDefault();
                if(improvementOnTile != null && improvementOnTile.IsComplete) {
                    baseYield += improvementOnTile.Template.BonusYield;
                }
            }

            return baseYield;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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
