using System;

using UnityEngine;

using Zenject;

using UnityCustomUtilities.Grids;

using Assets.Cities;

namespace Assets.GameMap {

    public interface IMapTile {

        #region properties

        HexCoords Coords { get; }

        Transform transform { get; }

        TerrainType Terrain { get; set; }
        TerrainShape Shape { get; set; }
        TerrainFeatureType Feature { get; set; }

        IWorkerSlot WorkerSlot { get; }

        #endregion

    }

}