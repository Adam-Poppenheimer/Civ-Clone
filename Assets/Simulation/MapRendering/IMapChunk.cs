using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public interface IMapChunk {

        #region properties

        IEnumerable<IHexCell> Cells { get; }

        #endregion

        #region methods

        void AttachCell(IHexCell cell);

        void InitializeTerrain(Vector3 position, float width, float height);

        void RefreshAlphamap();

        void RefreshAll();

        bool DoesCellOverlapChunk(IHexCell cell);

        bool IsInTerrainBounds2D(Vector2 positionXZ);

        Vector3 GetNearestPointOnTerrain(Vector3 fromLocation);

        #endregion

    }

}
