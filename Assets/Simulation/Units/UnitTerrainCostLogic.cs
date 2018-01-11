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

            int moveCost = Config.BaseLandMoveCost;

            if(edgeType == HexEdgeType.Slope && nextCell.Elevation > currentCell.Elevation) {
                moveCost += Config.SlopeMoveCost;
            }

            var featureCost = Config.FeatureMoveCosts[(int)nextCell.Feature];
            if(nextCell.Feature != TerrainFeature.None) {
                if(featureCost == -1) {
                    return -1;
                }else {
                    moveCost += featureCost;
                }
            }

            return moveCost;
        }

        #endregion
        
    }

}
