using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public interface IMapChunk {

        #region methods

        void InitializeTerrain(Vector3 position, float width, float height);

        void RefreshAlphamap();

        void RefreshAll();

        bool DoesCellOverlapChunk(IHexCell cell);

        #endregion

    }

}
