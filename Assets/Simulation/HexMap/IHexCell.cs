using System;

using UnityEngine;

using Zenject;

using Assets.UI.HexMap;
using Assets.Simulation.WorkerSlots;

namespace Assets.Simulation.HexMap {

    public interface IHexCell {

        #region properties

        Vector3 AbsolutePosition     { get; }
        Vector3 GridRelativePosition { get; }

        HexCoordinates Coordinates { get; }

        CellTerrain    Terrain    { get; set; }
        CellShape      Shape      { get; set; }
        CellVegetation Vegetation { get; set; }
        CellFeature    Feature    { get; set; }

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

        IWorkerSlot WorkerSlot { get; }

        bool SuppressSlot { get; set; }

        HexGridChunk Chunk { get; }

        int Index { get; }

        bool IsRoughTerrain { get; }

        Vector3 UnitAnchorPoint    { get; }
        Vector3 OverlayAnchorPoint { get; }

        #endregion

        #region methods

        int GetElevationDifference(HexDirection direction);

        void Refresh();
        void RefreshSelfOnly();

        void RefreshVisibility();

        void SetMapData(float data);

        #endregion

    }

}