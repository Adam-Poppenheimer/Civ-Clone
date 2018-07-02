namespace Assets.Simulation.HexMap {

    public interface IRiverTroughTriangulator {

        #region methods

        void CreateRiverTrough_Edge(CellTriangulationData data);

        void CreateRiverTrough_Confluence(CellTriangulationData data);

        void CreateRiverTrough_Curve_InnerEdge            (CellTriangulationData data);
        void CreateRiverTrough_Curve_OuterFlat            (CellTriangulationData data);
        void CreateRiverTrough_Curve_TerracesClockwiseDown(CellTriangulationData data);
        void CreateRiverTrough_Curve_TerracesClockwiseUp  (CellTriangulationData data);

        
        void CreateRiverTrough_Endpoint_CliffTerraces              (CellTriangulationData data);
        void CreateRiverTrough_Endpoint_DoubleCliff                (CellTriangulationData data);
        void CreateRiverTrough_Endpoint_DoubleFlat                 (CellTriangulationData data);
        void CreateRiverTrough_Endpoint_ShallowWaterRiverDelta     (CellTriangulationData data);
        void CreateRiverTrough_Endpoint_DoubleTerraces             (CellTriangulationData data);
        void CreateRiverTrough_Endpoint_FlatTerraces_ElevatedLeft  (CellTriangulationData data);
        void CreateRiverTrough_Endpoint_FlatTerraces_ElevatedRight (CellTriangulationData data);
        void CreateRiverTrough_Endpoint_TerracesCliff              (CellTriangulationData data);
        void CreateRiverTrough_Endpoint_TerracesFlat_ElevatedCenter(CellTriangulationData data);
        void CreateRiverTrough_Endpoint_TerracesFlat_ElevatedLeft  (CellTriangulationData data);

        #endregion

    }

}