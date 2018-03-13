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

        private IUnitPositionCanon UnitPositionCanon;

        private IPossessionRelationship<IHexCell, ICity> CityLocationCanon;

        #endregion

        #region constructors

        [Inject]
        public UnitTerrainCostLogic(
            IHexGridConfig config, IUnitPositionCanon unitPositionCanon,
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon
        ){
            Config            = config;
            UnitPositionCanon = unitPositionCanon;
            CityLocationCanon = cityLocationCanon;
        }

        #endregion

        #region instance methods

        #region from IUnitTerrainCostLogic

        public int GetTraversalCostForUnit(IUnit unit, IHexCell currentCell, IHexCell nextCell) {
            if(unit.Type == UnitType.City || !UnitPositionCanon.CanChangeOwnerOfPossession(unit, nextCell)) {
                return -1;
            }else if(unit.IsAquatic) {
                return GetAquaticTraversalCost(unit, currentCell, nextCell);
            }else {
                return GetNonAquaticTraversalCost(unit, currentCell, nextCell);
            }
        }

        #endregion

        private int GetAquaticTraversalCost(IUnit unit, IHexCell currentCell, IHexCell nextCell) {
            var cityAtNext = CityLocationCanon.GetPossessionsOfOwner(nextCell).FirstOrDefault();

            if(nextCell.IsUnderwater) {
                return Config.WaterMoveCost;
            }else if(cityAtNext != null) {
                return Config.WaterMoveCost;
            }else {
                return -1;
            }
        }

        private int GetNonAquaticTraversalCost(IUnit unit, IHexCell currentCell, IHexCell nextCell) {
            if(nextCell.IsUnderwater) {
                return -1;
            }

            var edgeType = HexMetrics.GetEdgeType(currentCell, nextCell);

            if(edgeType == HexEdgeType.Cliff){
                return -1;
            }

            int moveCost = Config.BaseLandMoveCost;

            if( edgeType == HexEdgeType.Slope && nextCell.EdgeElevation > currentCell.EdgeElevation &&
                !unit.Template.IgnoresTerrainCosts
            ) {
                moveCost += Config.SlopeMoveCost;
            }

            var featureCost = Config.FeatureMoveCosts[(int)nextCell.Feature];
            if(featureCost == -1) {
                return -1;
            }else if(!unit.Template.IgnoresTerrainCosts){
                moveCost += featureCost;
            }

            var shapeCost = Config.ShapeMoveCosts[(int)nextCell.Shape];
            if(nextCell.Shape != TerrainShape.Flatlands) {
                if(shapeCost == -1) {
                    return -1;
                }else if(!unit.Template.IgnoresTerrainCosts){
                    moveCost += shapeCost;
                }
            }

            if(IsCellRoughTerrain(nextCell) && unit.Template.HasRoughTerrainPenalty) {
                return unit.Template.MaxMovement;
            }

            return moveCost;
        }

        private bool IsCellRoughTerrain(IHexCell cell) {
            return !cell.HasRoads && (cell.Shape != TerrainShape.Flatlands || cell.Feature != TerrainFeature.None);
        }

        #endregion
        
    }

}
