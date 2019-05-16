using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.HexMap {

    public interface ICellModificationLogic {

        #region methods

        bool CanChangeTerrainOfCell(IHexCell cell, CellTerrain terrain);
        void ChangeTerrainOfCell   (IHexCell cell, CellTerrain terrain);

        bool CanChangeShapeOfCell(IHexCell cell, CellShape shape);
        void ChangeShapeOfCell   (IHexCell cell, CellShape shape);

        bool CanChangeVegetationOfCell(IHexCell cell, CellVegetation vegetation);
        void ChangeVegetationOfCell   (IHexCell cell, CellVegetation vegetation);

        bool CanChangeFeatureOfCell(IHexCell cell, CellFeature feature);
        void ChangeFeatureOfCell   (IHexCell cell, CellFeature feature);

        bool CanChangeHasRoadsOfCell(IHexCell cell, bool hasRoads);
        void ChangeHasRoadsOfCell   (IHexCell cell, bool hasRoads);

        #endregion

    }

}
