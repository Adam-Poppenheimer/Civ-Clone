using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public interface IMapRenderConfig {

        #region properties

        int RandomSeed { get; }

        Texture2D NoiseSource { get; }

        float NoiseScale { get; }

        int   NoiseHashGridSize  { get; }
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

        float MaxElevation { get; }

        int TerrainAlphamapResolution  { get; }
        int TerrainHeightmapResolution { get; }

        IEnumerable<Texture2D> MapTextures { get; }

        #endregion

        #region methods

        Vector3 GetFirstCorner (HexDirection direction);
        Vector3 GetSecondCorner(HexDirection direction);

        Vector3 GetFirstOuterSolidCorner (HexDirection direction);
        Vector3 GetSecondOuterSolidCorner(HexDirection direction);

        Vector3 GetOuterEdgeMidpoint(HexDirection direction);

        #endregion

    }

}
