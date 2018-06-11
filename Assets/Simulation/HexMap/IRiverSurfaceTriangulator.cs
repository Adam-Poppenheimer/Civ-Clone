namespace Assets.Simulation.HexMap {

    public interface IRiverSurfaceTriangulator {

        #region methods

        void CreateRiverSurface_Confluence(CellTriangulationData data);

        void CreateRiverSurface_EdgesAndCorners(
            CellTriangulationData thisData, CellTriangulationData nextData
        );

        void CreateRiverEndpointSurface_Default(CellTriangulationData data);

        void CreateRiverSurface_Waterfall(CellTriangulationData data, float yAdjustment);

        void CreateRiverSurface_Waterfall(
            CellTriangulationData data, float centerYAdjustment,
            float leftYAdjustment, float rightYAdjustment
        );

        #endregion

    }

}