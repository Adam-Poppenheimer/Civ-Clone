using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.Units {

    public class UnitTerrainCostLogic : IUnitTerrainCostLogic {

        #region instance fields and properties

        private IHexGridConfig Config;

        private ICityFactory CityFactory;

        #endregion

        #region constructors

        [Inject]
        public UnitTerrainCostLogic(IHexGridConfig config, ICityFactory cityFactory) {
            Config      = config;
            CityFactory = cityFactory;
        }

        #endregion

        #region instance methods

        #region from IUnitTerrainCostLogic

        public int GetTraversalCostForUnit(IUnit unit, IHexCell currentCell, IHexCell nextCell) {
            if(unit.Template.IsAquatic) {
                return GetAquaticTraversalCost(currentCell, nextCell);
            }else {
                return GetNonAquaticTraversalCost(currentCell, nextCell);
            }
        }

        #endregion

        private int GetAquaticTraversalCost(IHexCell currentCell, IHexCell nextCell) {
            if(nextCell.IsUnderwater || CityFactory.AllCities.Exists(city => city.Location == nextCell)) {
                return Config.WaterMoveCost;
            }else {
                return -1;
            }
        }

        private int GetNonAquaticTraversalCost(IHexCell currentCell, IHexCell nextCell) {
            var edgeType = HexMetrics.GetEdgeType(currentCell.Elevation, nextCell.Elevation);

            if( nextCell.IsUnderwater || edgeType == HexEdgeType.Cliff){
                return -1;
            }

            int moveCost = 0;

            switch(nextCell.Terrain) {
                case TerrainType.Grassland:    moveCost += Config.GrasslandMoveCost;    break;
                case TerrainType.Plains:       moveCost += Config.PlainsMoveCost;       break;
                case TerrainType.Desert:       moveCost += Config.DesertMoveCost;       break;
                default: break;
            }

            if(edgeType == HexEdgeType.Slope && nextCell.Elevation > currentCell.Elevation) {
                moveCost += Config.SlopeMoveCost;
            }

            switch(nextCell.Feature) {
                case TerrainFeature.Forest: moveCost += Config.ForestMoveCost; break;
                default: break;
            }

            return moveCost;
        }

        #endregion
        
    }

}
