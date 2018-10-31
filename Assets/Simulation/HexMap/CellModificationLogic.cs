using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Simulation.HexMap {

    public class CellModificationLogic : ICellModificationLogic {

        #region instance fields and properties

        private IRiverCanon RiverCanon;

        #endregion

        #region constructors

        [Inject]
        public CellModificationLogic(IRiverCanon riverCanon) {
            RiverCanon = riverCanon;
        }

        #endregion

        #region instance methods

        #region from ICellPropertyModificationLogic

        public bool CanChangeTerrainOfCell(IHexCell cell, CellTerrain terrain) {
            if(terrain == CellTerrain.FloodPlains) {
                return cell.Terrain == CellTerrain.Desert && cell.Shape == CellShape.Flatlands && RiverCanon.HasRiver(cell);
            }else {
                return true;
            }
        }

        public void ChangeTerrainOfCell(IHexCell cell, CellTerrain terrain) {
            if(!CanChangeTerrainOfCell(cell, terrain)) {
                throw new InvalidOperationException("CanChangeTerrainOfCell must return true on the given arguments");
            }

            cell.Terrain = terrain;

            if(terrain != CellTerrain.Grassland && cell.Vegetation == CellVegetation.Marsh) {
                ChangeVegetationOfCell(cell, CellVegetation.None);
            }

            if(cell.Vegetation == CellVegetation.Forest && (
                cell.Terrain == CellTerrain.Snow || cell.Terrain == CellTerrain.Desert ||
                cell.Terrain == CellTerrain.FloodPlains
            )) {
                ChangeVegetationOfCell(cell, CellVegetation.None);
            }

            if(cell.Vegetation == CellVegetation.Jungle && cell.Terrain != CellTerrain.Grassland && cell.Terrain != CellTerrain.Plains) {
                ChangeVegetationOfCell(cell, CellVegetation.None);
            }

            if(cell.Terrain != CellTerrain.Desert && cell.Feature == CellFeature.Oasis) {
                ChangeFeatureOfCell(cell, CellFeature.None);
            }

            if(cell.Terrain.IsWater()) {
                ChangeVegetationOfCell(cell, CellVegetation.None);
                ChangeShapeOfCell(cell, CellShape.Flatlands);

                RiverCanon.RemoveAllRiversFromCell(cell);

                ChangeHasRoadsOfCell(cell, false);
            }
        }

        public bool CanChangeShapeOfCell(IHexCell cell, CellShape shape) {
            return true;
        }

        public void ChangeShapeOfCell(IHexCell cell, CellShape shape) {
            if(!CanChangeShapeOfCell(cell, shape)) {
                throw new InvalidOperationException("CanChangeShapeOfCell must return true on the given arguments");
            }

            cell.Shape = shape;

            if(shape != CellShape.Flatlands) {
                if(cell.Terrain == CellTerrain.FloodPlains) {
                    ChangeTerrainOfCell(cell, CellTerrain.Desert);
                }

                if(cell.Vegetation == CellVegetation.Marsh) {
                    ChangeVegetationOfCell(cell, CellVegetation.None);
                }

                if(cell.Terrain.IsWater()) {
                    ChangeTerrainOfCell(cell, CellTerrain.Grassland);
                }

                if(cell.Vegetation != CellVegetation.None && shape == CellShape.Mountains) {
                    ChangeVegetationOfCell(cell, CellVegetation.None);
                }

                if(cell.Feature == CellFeature.Oasis) {
                    ChangeFeatureOfCell(cell, CellFeature.None);
                }

                if(shape == CellShape.Mountains && cell.HasRoads) {
                    ChangeHasRoadsOfCell(cell, false);
                }
            }
        }

        public bool CanChangeVegetationOfCell(IHexCell cell, CellVegetation vegetation) {
            if(cell.Terrain.IsWater() && vegetation != CellVegetation.None && vegetation != CellVegetation.Marsh) {
                return false;
            }else if(vegetation == CellVegetation.Forest) {
                return cell.Terrain != CellTerrain.Desert && cell.Terrain != CellTerrain.FloodPlains &&
                       cell.Terrain != CellTerrain.Snow   && cell.Shape != CellShape.Mountains;
            }else if(vegetation == CellVegetation.Jungle) {
                return (cell.Terrain == CellTerrain.Grassland || cell.Terrain == CellTerrain.Plains)
                    && cell.Shape != CellShape.Mountains;
            }else {
                return true;
            }
        }

        public void ChangeVegetationOfCell(IHexCell cell, CellVegetation vegetation) {
            if(!CanChangeVegetationOfCell(cell, vegetation)) {
                throw new InvalidOperationException("CanChangeVegetationOfCell must return true on the given arguments");
            }

            cell.Vegetation = vegetation;

            if(vegetation == CellVegetation.Marsh) {
                ChangeTerrainOfCell(cell, CellTerrain.Grassland);
                ChangeShapeOfCell  (cell, CellShape.Flatlands);
            }
        }

        public bool CanChangeFeatureOfCell(IHexCell cell, CellFeature feature) {
            if(feature == CellFeature.None) {
                return true;

            }else if(feature == CellFeature.Oasis) {
                return cell.Terrain == CellTerrain.Desert && cell.Shape == CellShape.Flatlands;

            }else if(feature == CellFeature.CityRuins) {
                return true;

            } else {
                return false;
            }
        }

        public void ChangeFeatureOfCell(IHexCell cell, CellFeature feature) {
            if(!CanChangeFeatureOfCell(cell, feature)) {
                throw new InvalidOperationException("CanChangeFeatureOfCell must return true on the given arguments");
            }

            cell.Feature = feature;
        }

        public bool CanChangeHasRoadsOfCell(IHexCell cell, bool hasRoads) {
            return !hasRoads || (!cell.Terrain.IsWater() && cell.Shape != CellShape.Mountains);
        }

        public void ChangeHasRoadsOfCell(IHexCell cell, bool hasRoads) {
            if(!CanChangeHasRoadsOfCell(cell, hasRoads)) {
                throw new InvalidOperationException("CanChangeHasRoadsOfCell must return true on the given arguments");
            }

            cell.HasRoads = hasRoads;
        }

        #endregion

        #endregion

    }

}
