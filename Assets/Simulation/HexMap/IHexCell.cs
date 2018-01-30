using System;

using UnityEngine;

using Zenject;

using Assets.UI.HexMap;

using UnityCustomUtilities.Grids;

namespace Assets.Simulation.HexMap {

    public interface IHexCell {

        #region properties

        HexCoordinates Coordinates { get; }

        Transform transform { get; }

        TerrainType    Terrain { get; set; }
        TerrainFeature Feature { get; set; }

        int Elevation { get; set; }

        float StreamBedY { get; }
        float RiverSurfaceY { get; }

        bool HasRoads { get; }

        int WaterLevel { get; set; }

        float WaterSurfaceY { get; }

        bool IsUnderwater { get; }

        IWorkerSlot WorkerSlot { get; }

        bool SuppressSlot { get; set; }

        HexGridChunk Chunk { get; }

        IHexCellOverlay Overlay { get; }

        #endregion

        #region methods

        HexEdgeType GetEdgeType(IHexCell otherCell);

        bool HasRoadThroughEdge(HexDirection direction);

        void AddRoad(HexDirection direction);

        void RemoveRoads();

        int GetElevationDifference(HexDirection direction);

        void Refresh();
        void RefreshSelfOnly();

        #endregion

    }

}