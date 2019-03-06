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

        INoiseTexture GenericNoiseSource { get; }

        float NoiseScale { get; }

        int NoiseHashGridSize { get; }
        float NoiseHashGridScale { get; }

        float CellPerturbStrengthXZ { get; }


        float OuterToInner { get; }
        float InnerToOuter { get; }

        float OuterRadius { get; }
        float InnerRadius { get; }

        float OuterSolidFactor { get; }
        float InnerSolidFactor { get; }

        float BlendFactor { get; }

        IEnumerable<Vector3> Corners { get; }

        float ChunkWidth { get; }
        float ChunkHeight { get; }

        float TerrainMaxY { get; }

        int TerrainAlphamapResolution  { get; }
        int TerrainHeightmapResolution { get; }

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

        INoiseTexture FlatlandsElevationHeightmap { get; }
        INoiseTexture HillsElevationHeightmap     { get; }

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
