﻿using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.UI.HexMap;
using Assets.Simulation.WorkerSlots;
using Assets.Simulation.MapRendering;

namespace Assets.Simulation.HexMap {

    public interface IHexCell {

        #region properties

        Vector3 AbsolutePosition     { get; }
        Vector2 AbsolutePositionXZ   { get; }
        Vector3 GridRelativePosition { get; }

        HexCoordinates Coordinates { get; }

        CellTerrain    Terrain    { get; set; }
        CellShape      Shape      { get; set; }
        CellVegetation Vegetation { get; set; }
        CellFeature    Feature    { get; set; }

        bool HasRoads { get; set; }

        IWorkerSlot WorkerSlot { get; }

        bool SuppressSlot { get; set; }

        int Index { get; }

        bool IsRoughTerrain { get; }

        Vector3 OverlayAnchorPoint { get; }

        IEnumerable<IMapChunk> OverlappingChunks { get; }

        #endregion

        #region methods

        void AttachToChunks(IEnumerable<IMapChunk> chunks);

        void SetMapData(float data);

        #endregion

    }

}