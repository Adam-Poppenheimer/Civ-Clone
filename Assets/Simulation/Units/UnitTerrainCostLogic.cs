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

        private IUnitPositionCanon UnitPositionCanon;

        #endregion

        #region constructors

        [Inject]
        public UnitTerrainCostLogic(IHexGridConfig config, ICityFactory cityFactory,
            IUnitPositionCanon unitPositionCanon
        ){
            Config            = config;
            CityFactory       = cityFactory;
            UnitPositionCanon = unitPositionCanon;
        }

        #endregion

        #region instance methods

        #region from IUnitTerrainCostLogic

        public int GetTraversalCostForUnit(IUnit unit, IHexCell currentCell, IHexCell nextCell) {
            if(!UnitPositionCanon.CanChangeOwnerOfPossession(unit, nextCell)) {
                return -1;
            }else if(unit.IsAquatic) {
                return GetAquaticTraversalCost(currentCell, nextCell);
            }else {
                return GetNonAquaticTraversalCost(currentCell, nextCell);
            }
        }

        #endregion

        private int GetAquaticTraversalCost(IHexCell currentCell, IHexCell nextCell) {
            if(nextCell.IsUnderwater) {
                return Config.WaterMoveCost;
            }else if(CityFactory.AllCities.Exists(city => city.Location == nextCell)) {
                return Config.WaterMoveCost;
            }else {
                return -1;
            }
        }

        private int GetNonAquaticTraversalCost(IHexCell currentCell, IHexCell nextCell) {
            if(nextCell.IsUnderwater) {
                return -1;
            }

            var edgeType = HexMetrics.GetEdgeType(currentCell, nextCell);

            if(edgeType == HexEdgeType.Cliff){
                return -1;
            }

            int moveCost = Config.BaseLandMoveCost;

            if(edgeType == HexEdgeType.Slope && nextCell.EdgeElevation > currentCell.EdgeElevation) {
                moveCost += Config.SlopeMoveCost;
            }

            var featureCost = Config.FeatureMoveCosts[(int)nextCell.Feature];
            if(featureCost == -1) {
                return -1;
            }else {
                moveCost += featureCost;
            }

            var shapeCost = Config.ShapeMoveCosts[(int)nextCell.Shape];
            if(nextCell.Shape != TerrainShape.Flatlands) {
                if(shapeCost == -1) {
                    return -1;
                }else {
                    moveCost += shapeCost;
                }
            }

            return moveCost;
        }

        #endregion
        
    }

}
