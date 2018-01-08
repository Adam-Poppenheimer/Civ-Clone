using System;

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

        bool HasRiver { get; }
        bool HasRiverBeginOrEnd { get; }

        bool HasIncomingRiver { get; set; }
        bool HasOutgoingRiver { get; set; }

        HexDirection IncomingRiver { get; set; }
        HexDirection OutgoingRiver { get; set; }

        float StreamBedY { get; }
        float RiverSurfaceY { get; }

        Color Color { get; set; }

        IWorkerSlot WorkerSlot { get; }

        bool SuppressSlot { get; set; }

        HexGridChunk Chunk { get; }

        #endregion

        #region methods

        HexEdgeType GetEdgeType(IHexCell otherCell);

        bool HasRiverThroughEdge(HexDirection direction);

        void SetOutgoingRiver(HexDirection direction);

        void RemoveOutgoingRiver();
        void RemoveIncomingRiver();

        void RemoveRiver();

        void Refresh();
        void RefreshSelfOnly();

        #endregion

    }

}