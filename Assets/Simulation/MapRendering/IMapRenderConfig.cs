﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public interface IMapRenderConfig {

        #region properties

        int RandomSeed { get; }

        float NoiseScale { get; }

        INoiseTexture GenericNoiseSource            { get; }
        INoiseTexture FlatlandsElevationNoiseSource { get; }
        INoiseTexture HillsElevationNoiseSource     { get; }

        int   NoiseHashGridSize  { get; }
        float NoiseHashGridScale { get; }

        float CellPerturbStrengthXZ { get; }

        float FlatlandsElevationNoiseStrength { get; }
        float HillsElevationNoiseStrength     { get; }


        float OuterToInner { get; }
        float InnerToOuter { get; }

        float OuterRadius { get; }
        float InnerRadius { get; }

        float OuterSolidFactor { get; }
        float InnerSolidFactor { get; }

        float BlendFactor { get; }

        IEnumerable<Vector3> Corners   { get; }
        IEnumerable<Vector2> CornersXZ { get; }

        float ChunkWidth { get; }
        float ChunkHeight { get; }

        float TerrainMaxY { get; }

        int TerrainAlphamapResolution  { get; }
        int TerrainHeightmapResolution { get; }

        LayerMask MapCollisionMask { get; }

        Material TerrainMaterial { get; }

        IEnumerable<Texture2D> MapTextures { get; }

        int SeaFloorTextureIndex { get; }
        int MountainTextureIndex { get; }

        float WaterSurfaceElevation  { get; }
        float SeaFloorElevation      { get; }
        float FlatlandsBaseElevation { get; }
        float HillsBaseElevation     { get; }
        float MountainPeakElevation  { get; }
        float MountainRidgeElevation { get; }
        float RiverTroughElevation   { get; }

        float WaterY { get; }        

        Color ShallowWaterColor { get; }
        Color DeepWaterColor    { get; }
        Color FreshWaterColor   { get; }
        Color RiverWaterColor   { get; }

        float RiverMaxWidth      { get; }
        float RiverCurveStrength { get; }
        float RiverWideningRate  { get; }
        float RiverWidthNoise    { get; }

        int RiverCurvesForMaxWidth { get; }
        int RiverQuadsPerCurve     { get; }

        float RiverFlowSpeed { get; }

        //May need to be retyped to prevent unintentional modification
        float[] RiverAlphamap { get; }

        float CultureWidthPercent { get; }

        float CultureTriangleSideWidth { get; }


        HexMeshData StandingWaterData { get; }
        HexMeshData RiversData         { get; }
        HexMeshData CultureData       { get; }

        #endregion

        #region methods

        Vector3 GetFirstCorner (HexDirection direction);
        Vector3 GetSecondCorner(HexDirection direction);

        Vector3 GetFirstSolidCorner (HexDirection direction);
        Vector3 GetSecondSolidCorner(HexDirection direction);

        Vector3 GetEdgeMidpoint(HexDirection direction);

        Vector3 GetSolidEdgeMidpoint(HexDirection direction);


        Vector2 GetFirstCornerXZ (HexDirection direction);
        Vector2 GetSecondCornerXZ(HexDirection direction);

        Vector2 GetFirstSolidCornerXZ (HexDirection direction);
        Vector2 GetSecondSolidCornerXZ(HexDirection direction);

        Vector2 GetEdgeMidpointXZ(HexDirection direction);

        Vector2 GetSolidEdgeMidpointXZ(HexDirection direction);

        #endregion

    }

}
