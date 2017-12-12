using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.GameMap;
using Assets.Simulation.Cities;

namespace Assets.Simulation.Units {

    public class UnitPositionCanon : PossessionRelationship<IMapTile, IUnit> {

        #region instance fields and properties

        private List<TerrainType> LandTerrainTypes;

        private List<TerrainShape> ImpassableTerrainShapes;

        private IRecordkeepingCityFactory CityFactory;

        #endregion

        #region constructors

        [Inject]
        public UnitPositionCanon(
            [Inject(Id = "Land Terrain Types")] List<TerrainType> landTerrainTypes,
            [Inject(Id = "Impassable Terrain Types")] List<TerrainShape> impassableTerrainShapes,
            IRecordkeepingCityFactory cityFactory
        ){
            LandTerrainTypes = landTerrainTypes;
            ImpassableTerrainShapes = impassableTerrainShapes;
            CityFactory = cityFactory;
        }

        #endregion

        #region instance methods

        #region from PossessionRelationship<IMapTile, IUnit>

        protected override bool IsPossessionValid(IUnit unit, IMapTile location) {
            var unitType = unit.Template.Type;

            if(unitType == UnitType.WaterMilitary || unitType == UnitType.WaterCivilian) {
                return IsValidForWaterUnit(location) && !AlreadyHasUnitOfType(location, unitType);
            }else {
                return IsValidForLandUnit(location) && !AlreadyHasUnitOfType(location, unitType);
            }
        }

        #endregion

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
