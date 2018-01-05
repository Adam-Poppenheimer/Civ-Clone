﻿using System;

using UnityEngine;

using Zenject;

using UnityCustomUtilities.Grids;

namespace Assets.Simulation.HexMap {

    public interface IHexCell {

        #region properties

        HexCoordinates Coordinates { get; }

        Transform transform { get; }

        TerrainType    Terrain { get; set; }
        TerrainShape   Shape   { get; set; }
        TerrainFeature Feature { get; set; }

        int Elevation { get; set; }

        Color Color { get; set; }

        IWorkerSlot WorkerSlot { get; }

        bool SuppressSlot { get; set; }

        HexGridChunk Chunk { get; }

        #endregion

        #region methods

        HexEdgeType GetEdgeType(IHexCell otherCell);

        void Refresh();

        #endregion

    }

}