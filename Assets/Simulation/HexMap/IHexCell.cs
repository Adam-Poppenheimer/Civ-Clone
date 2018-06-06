﻿using System;

using UnityEngine;

using Zenject;

using Assets.UI.HexMap;
using Assets.Simulation.WorkerSlots;

namespace Assets.Simulation.HexMap {

    public interface IHexCell {

        #region properties

        Vector3 Position      { get; }
        Vector3 LocalPosition { get; }

        HexCoordinates Coordinates { get; }

        TerrainType    Terrain { get; set; }
        TerrainFeature Feature { get; set; }
        TerrainShape   Shape   { get; set; }

        int FoundationElevation { get; }
        int EdgeElevation       { get; }
        int PeakElevation       { get; }

        float PeakY         { get; }
        float EdgeY         { get; }
        float StreamBedY    { get; }
        float RiverSurfaceY { get; }

        bool RequiresYPerturb { get; }

        bool HasRoads { get; set; }

        float WaterSurfaceY { get; }

        bool IsUnderwater { get; }

        int ViewElevation { get; }

        IWorkerSlot WorkerSlot { get; }

        bool SuppressSlot { get; set; }

        HexGridChunk Chunk { get; }

        int Index { get; }

        IHexCellOverlay Overlay { get; }

        bool IsRoughTerrain { get; }

        Vector3 UnitAnchorPoint { get; }

        #endregion

        #region methods

        int GetElevationDifference(HexDirection direction);

        void Refresh();
        void RefreshSelfOnly();

        void RefreshVisibility();
        void RefreshElevation();

        void Destroy();

        #endregion

    }

}