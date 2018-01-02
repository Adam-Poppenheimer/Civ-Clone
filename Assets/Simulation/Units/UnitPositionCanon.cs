using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.GameMap;
using Assets.Simulation.Cities;

namespace Assets.Simulation.Units {

    public class UnitPositionCanon : PossessionRelationship<IMapTile, IUnit>, IUnitPositionCanon {

        #region instance fields and properties

        private List<TerrainType> LandTerrainTypes;

        private List<TerrainShape> ImpassableTerrainShapes;

        private ICityFactory CityFactory;

        private UnitSignals Signals;

        #endregion

        #region constructors

        [Inject]
        public UnitPositionCanon(
            [Inject(Id = "Land Terrain Types")] List<TerrainType> landTerrainTypes,
            [Inject(Id = "Impassable Terrain Types")] List<TerrainShape> impassableTerrainShapes,
            ICityFactory cityFactory, UnitSignals signals
        ){
            LandTerrainTypes        = landTerrainTypes;
            ImpassableTerrainShapes = impassableTerrainShapes;
            CityFactory             = cityFactory;
            Signals                 = signals;
        }

        #endregion

        #region instance methods

        #region from PossessionRelationship<IMapTile, IUnit>

        protected override bool IsPossessionValid(IUnit unit, IMapTile location) {
            return location == null || CanPlaceUnitOfTypeAtLocation(unit.Template.Type, location);            
        }

        protected override void DoOnPossessionBroken(IUnit possession, IMapTile oldOwner) {
            possession.gameObject.transform.SetParent(null, false);
        }

        protected override void DoOnPossessionEstablished(IUnit possession, IMapTile newOwner) {
            possession.gameObject.transform.SetParent(newOwner != null ? newOwner.transform : null, false);
            Signals.UnitLocationChangedSignal.OnNext(new UniRx.Tuple<IUnit, IMapTile>(possession, newOwner));
        }

        #endregion

        public bool CanPlaceUnitOfTypeAtLocation(UnitType type, IMapTile location) {
            if(location == null) {
                return true;
            }else if(type == UnitType.WaterMilitary || type == UnitType.WaterCivilian) {
                return IsValidForWaterUnit(location) && !AlreadyHasUnitOfType(location, type);
            }else {
                return IsValidForLandUnit(location) && !AlreadyHasUnitOfType(location, type);
            }
        }

        private bool IsValidForWaterUnit(IMapTile tile) {
            return (
                !LandTerrainTypes.Contains(tile.Terrain)
                || CityFactory.AllCities.Where(city => city.Location == tile).LastOrDefault() != null
            ) && !ImpassableTerrainShapes.Contains(tile.Shape);
        }

        private bool IsValidForLandUnit(IMapTile tile) {
            return LandTerrainTypes.Contains(tile.Terrain)
                && !ImpassableTerrainShapes.Contains(tile.Shape);
        }

        private bool AlreadyHasUnitOfType(IMapTile owner, UnitType type) {
            return GetPossessionsOfOwner(owner).Select(unit => unit.Template.Type).Contains(type);
        }

        #endregion

    }

}
