using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Civilizations;
using Assets.Simulation.GameMap;
using Assets.Simulation.Improvements;

namespace Assets.Simulation.Cities.ResourceGeneration {

    public class IncomeModifierLogic : IIncomeModifierLogic {

        #region instance fields and properties

        private IBuildingPossessionCanon BuildingPossessionCanon;

        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        private IMapHexGrid Map;

        private IImprovementLocationCanon ImprovementLocationCanon;

        #endregion

        #region constructors

        [Inject]
        public IncomeModifierLogic(
            IBuildingPossessionCanon buildingPossessionCanon,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IImprovementLocationCanon improvementLocationCanon,
            IMapHexGrid map
        ){
            BuildingPossessionCanon  = buildingPossessionCanon;
            CityPossessionCanon      = cityPossessionCanon;
            ImprovementLocationCanon = improvementLocationCanon;
            Map                      = map;
        }

        #endregion

        #region instance methods

        #region from IIncomeModifierLogic

        public ResourceSummary GetRealBaseYieldForSlot(IWorkerSlot slot) {
            if(slot == null) {
                throw new ArgumentNullException("slot");
            }

            var baseYield = slot.BaseYield;

            var tileOfSlot = Map.Tiles.Where(tile => tile.WorkerSlot == slot).FirstOrDefault();

            if(tileOfSlot != null) {
                var improvementOnTile = ImprovementLocationCanon.GetPossessionsOfOwner(tileOfSlot).FirstOrDefault();
                if(improvementOnTile != null) {
                    baseYield += improvementOnTile.Template.BonusYield;
                }
            }

            return baseYield;
        }

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
