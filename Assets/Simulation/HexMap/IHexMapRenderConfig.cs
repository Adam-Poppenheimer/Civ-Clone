using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.HexMap {

    public interface IHexMapRenderConfig {

        #region properties

        int RandomSeed { get; }

        Texture2D NoiseSource { get; }

        float NoiseScale { get; }

        int   NoiseHashGridSize  { get; }
        float NoiseHashGridScale { get; }

        int WaterLevel { get; }

        int MaxElevation { get; }

        float OuterToInner { get; }
        float InnerToOuter { get; }

        float OuterRadius { get; }
        float InnerRadius { get; }

        float OuterSolidFactor { get; }
        float InnerSolidFactor { get; }

        float BlendFactor { get; }

        float ElevationStep { get; }

        int TerracesPerSlope { get; }

        int TerraceSteps { get; }

        float HorizontalTerraceStepSize { get; }
        float VerticalTerraceStepSize   { get; }

        float CellPerturbStrengthXZ { get; }
        float CellPerturbStrengthY  { get; }

        float MinHillPerturbation { get; }
        
        int ChunkSizeX { get; }
        int ChunkSizeZ { get; }

        float StreamBedElevationOffset { get; }

        float OceanElevationOffset { get; }
        float RiverElevationOffset { get; }

        float WaterFactor      { get; }
        float WaterBlendFactor { get; }

        float RiverTroughEndpointPullback { get; }

        float RiverPortU        { get; }
        float RiverStarboardU   { get; }

        float RiverEndpointV    { get; }
        float RiverEdgeStartV   { get; }
        float RiverEdgeStepV    { get; }
        float RiverEdgeEndV     { get; }
        
        float RiverConfluenceV  { get; }
        float EstuaryWaterfallV { get; }
        float RiverWaterfallV   { get; }

        float CornerWaterfallTroughProtrusion { get; }

        float OasisWaterLerp       { get; }
        float OasisVerdantEdgeLerp { get; }

        int WaterTerrainIndex    { get; }
        int MountainTerrainIndex { get; }

        float CultureStartFactor { get; }

        #endregion

        #region methods

        Vector3 GetFirstCorner (HexDirection direction);
        Vector3 GetSecondCorner(HexDirection direction);

        Vector3 GetFirstOuterSolidCorner (HexDirection direction);
        Vector3 GetSecondOuterSolidCorner(HexDirection direction);

        Vector3 GetFirstInnerSolidCorner (HexDirection direction);
        Vector3 GetSecondInnerSolidCorner(HexDirection direction);

        Vector3 GetFirstWaterCorner (HexDirection direction);
        Vector3 GetSecondWaterCorner(HexDirection direction);

        Vector3 GetFirstCultureStartCorner (HexDirection direction);
        Vector3 GetSecondCultureStartCorner(HexDirection direction);

        Vector3      TerraceLerp(Vector3      a, Vector3      b, int step);
        Color        TerraceLerp(Color        a, Color        b, int step);
        float        TerraceLerp(float        a, float        b, int step);
        EdgeVertices TerraceLerp(EdgeVertices a, EdgeVertices b, int step);

        Vector3 GetOuterEdgeMiddle(HexDirection direction);

        float GetRiverEdgeV(int vertex);

        int GetFoundationElevationForTerrain(CellTerrain terrain);
        int GetEdgeElevationForShape        (CellShape shape);
        int GetPeakElevationForShape        (CellShape shape);

        #endregion

    }

}
