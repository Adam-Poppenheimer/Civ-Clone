﻿namespace Assets.Simulation.HexMap {

    public interface IRiverSurfaceTriangulator {

        #region methods

        void CreateRiverSurface_Confluence(CellTriangulationData data);

        void CreateRiverSurface_ThisCornerToMiddle(CellTriangulationData thisData);
        void CreateRiverSurface_MiddleToNextCorner(CellTriangulationData nextData);
        void CreateRiverSurface_CurveCorner       (CellTriangulationData thisData);

        void CreateRiverEndpointSurface_Default(CellTriangulationData data);

        void CreateRiverSurface_ConfluenceWaterfall(CellTriangulationData data, float yAdjustment);

        void CreateRiverSurface_ConfluenceWaterfall(
            CellTriangulationData data, float centerYAdjustment,
            float leftYAdjustment, float rightYAdjustment
        );

        void CreateRiverSurface_EstuaryWaterfall(CellTriangulationData data, float yAdjustment);

        #endregion

    }

}