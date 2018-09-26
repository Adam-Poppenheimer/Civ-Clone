using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;
using Assets.Simulation.Units.Promotions;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Units {

    public class UnitTerrainCostLogic : IUnitTerrainCostLogic {

        #region instance fields and properties

        private IHexMapSimulationConfig                       Config;
        private IUnitPositionCanon                            UnitPositionCanon;
        private IPossessionRelationship<IHexCell, ICity>      CityLocationCanon;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private IPromotionParser                              PromotionParser;

        #endregion

        #region constructors

        [Inject]
        public UnitTerrainCostLogic(
            IHexMapSimulationConfig config, IUnitPositionCanon unitPositionCanon,
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            IPromotionParser promotionParser
        ){
            Config              = config;
            UnitPositionCanon   = unitPositionCanon;
            CityLocationCanon   = cityLocationCanon;
            CityPossessionCanon = cityPossessionCanon;
            UnitPossessionCanon = unitPossessionCanon;
            PromotionParser     = promotionParser;
        }

        #endregion

        #region instance methods

        #region from IUnitTerrainCostLogic

        public float GetTraversalCostForUnit(IUnit unit, IHexCell currentCell, IHexCell nextCell) {
            var cityAtNext = CityLocationCanon.GetPossessionsOfOwner(nextCell).FirstOrDefault();
            var ownerOf = UnitPossessionCanon.GetOwnerOfPossession(unit);

            if(unit.Type == UnitType.City || !UnitPositionCanon.CanChangeOwnerOfPossession(unit, nextCell)) {
                return -1;
            }else if(cityAtNext != null) {
                return ownerOf == CityPossessionCanon.GetOwnerOfPossession(cityAtNext) ? Config.CityMoveCost : -1;
            } else if(unit.IsAquatic) {
                return GetAquaticTraversalCost(unit, currentCell, nextCell);
            }else {
                return GetNonAquaticTraversalCost(unit, currentCell, nextCell);
            }
        }

        #endregion

        private float GetAquaticTraversalCost(IUnit unit, IHexCell currentCell, IHexCell nextCell) {
            if(nextCell.Terrain.IsWater()) {
                return Config.GetBaseMoveCostOfTerrain(nextCell.Terrain);
            }else {
                return -1;
            }
        }

        private float GetNonAquaticTraversalCost(IUnit unit, IHexCell currentCell, IHexCell nextCell) {
            if(nextCell.Terrain.IsWater()) {
                return -1;
            }

            var movementInfo = PromotionParser.GetMovementInfo(unit);

            int moveCost = Config.GetBaseMoveCostOfTerrain(nextCell.Terrain);

            if(currentCell.HasRoads && nextCell.HasRoads) {
                return moveCost * Config.RoadMoveCostMultiplier;
            }

            var featureCost = Config.GetBaseMoveCostOfVegetation(nextCell.Vegetation);
            if(featureCost == -1) {
                return -1;
            }else if(!movementInfo.IgnoresTerrainCosts){
                moveCost += featureCost;
            }

            var shapeCost = Config.GetBaseMoveCostOfShape(nextCell.Shape);
            if(nextCell.Shape != CellShape.Flatlands) {
                if(shapeCost == -1) {
                    return -1;
                }else if(!movementInfo.IgnoresTerrainCosts){
                    moveCost += shapeCost;
                }
            }

            if(IsCellRoughTerrain(nextCell) && movementInfo.HasRoughTerrainPenalty) {
                return unit.Template.MaxMovement;
            }

            return moveCost;
        }

        private bool IsCellRoughTerrain(IHexCell cell) {
            return !cell.HasRoads && (cell.Shape != CellShape.Flatlands || cell.Vegetation != CellVegetation.None);
        }

        #endregion
        
    }

}
