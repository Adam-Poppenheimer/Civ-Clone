﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;

namespace Assets.Simulation.Units {

    public class UnitPositionCanon : PossessionRelationship<IHexCell, IUnit>, IUnitPositionCanon {

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

        protected override bool IsPossessionValid(IUnit unit, IHexCell location) {
            return location == null || CanPlaceUnitOfTypeAtLocation(unit.Template.Type, location);            
        }

        protected override void DoOnPossessionBroken(IUnit possession, IHexCell oldOwner) {
            possession.gameObject.transform.SetParent(null, false);
        }

        protected override void DoOnPossessionEstablished(IUnit possession, IHexCell newOwner) {
            possession.gameObject.transform.SetParent(newOwner != null ? newOwner.transform : null, false);
            Signals.UnitLocationChangedSignal.OnNext(new UniRx.Tuple<IUnit, IHexCell>(possession, newOwner));
        }

        #endregion

        public bool CanPlaceUnitOfTypeAtLocation(UnitType type, IHexCell location) {
            if(location == null) {
                return true;
            }else if(type == UnitType.WaterMilitary || type == UnitType.WaterCivilian) {
                return IsValidForWaterUnit(location) && !AlreadyHasUnitOfType(location, type);
            }else {
                return IsValidForLandUnit(location) && !AlreadyHasUnitOfType(location, type);
            }
        }

        private bool IsValidForWaterUnit(IHexCell tile) {
            return (
                !LandTerrainTypes.Contains(tile.Terrain)
                || CityFactory.AllCities.Where(city => city.Location == tile).LastOrDefault() != null
            ) && !ImpassableTerrainShapes.Contains(tile.Shape);
        }

        private bool IsValidForLandUnit(IHexCell tile) {
            return LandTerrainTypes.Contains(tile.Terrain)
                && !ImpassableTerrainShapes.Contains(tile.Shape);
        }

        private bool AlreadyHasUnitOfType(IHexCell owner, UnitType type) {
            return GetPossessionsOfOwner(owner).Select(unit => unit.Template.Type).Contains(type);
        }

        #endregion

    }

}
