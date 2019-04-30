using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public interface IMapChunk {

        #region properties

        Transform transform { get; }

        IEnumerable<IHexCell> CenteredCells    { get; }
        IEnumerable<IHexCell> OverlappingCells { get; }

        Terrain Terrain { get; }

        float Width  { get; }
        float Height { get; }

        Texture2D LandBakeTexture  { get; }
        Texture2D WaterBakeTexture { get; }

        bool IsRefreshing { get; }

        #endregion

        #region methods

        void AttachCell(IHexCell cell, bool isCentered);

        void Initialize(Vector3 position, float width, float height);

        void Refresh(TerrainRefreshType refreshTypes);

        bool DoesCellOverlapChunk(IHexCell cell);

        bool IsInTerrainBounds2D(Vector2 positionXZ);

        Vector3 GetNearestPointOnTerrain(Vector3 fromLocation);

        #endregion

    }

}
