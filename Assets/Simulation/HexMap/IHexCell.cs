using System;

using UnityEngine;

using Zenject;

using Assets.UI.HexMap;
using Assets.Simulation.WorkerSlots;

namespace Assets.Simulation.HexMap {

    public interface IHexCell {

        #region properties

        HexCoordinates Coordinates { get; }

        Transform transform { get; }

        TerrainType    Terrain { get; set; }
        TerrainFeature Feature { get; set; }
        TerrainShape   Shape   { get; set; }

        int FoundationElevation { get; set; }
        int EdgeElevation { get; }
        int PeakElevation { get; }

        float StreamBedY { get; }
        float RiverSurfaceY { get; }

        bool HasRoads { get; set; }

        int WaterLevel { get; set; }

        float WaterSurfaceY { get; }

        bool IsUnderwater { get; }

        IWorkerSlot WorkerSlot { get; }

        bool SuppressSlot { get; set; }

        HexGridChunk Chunk { get; }

        int Index { get; }

        IHexCellOverlay Overlay { get; }

        #endregion

        #region methods

        HexEdgeType GetEdgeType(IHexCell otherCell);

        int GetElevationDifference(HexDirection direction);

        void Refresh();
        void RefreshSelfOnly();

        void RefreshVisibility();

        #endregion

    }

}