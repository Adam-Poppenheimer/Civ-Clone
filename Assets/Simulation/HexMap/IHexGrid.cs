using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.HexMap {

    public interface IHexGrid {

        #region properties

        ReadOnlyCollection<IHexCell> Cells { get; }

        int ChunkCountX { get; }
        int ChunkCountZ { get; }

        int CellCountX { get; }
        int CellCountZ { get; }

        #endregion

        #region methods

        void Build(int chunkCountX, int chunkCountZ);
        void Clear();

        bool     HasCellAtCoordinates(HexCoordinates coords);
        IHexCell GetCellAtCoordinates(HexCoordinates coords);

        bool     HasCellAtLocation(Vector3 location);
        IHexCell GetCellAtLocation(Vector3 location);

        IHexCell GetCellAtOffset(int xOffset, int zOffset);

        bool     HasNeighbor(IHexCell center, HexDirection direction);
        IHexCell GetNeighbor(IHexCell center, HexDirection direction);

        int GetDistance(IHexCell cellOne, IHexCell cellTwo);

        List<IHexCell> GetCellsInRadius(IHexCell center, int radius);

        List<IHexCell> GetCellsInLine(IHexCell start, IHexCell end);

        List<IHexCell> GetCellsInRing(IHexCell center, int radius);

        List<IHexCell> GetShortestPathBetween(IHexCell start, IHexCell end);

        List<IHexCell> GetShortestPathBetween(IHexCell start, IHexCell end, Func<IHexCell, IHexCell, float> costFunction);

        List<IHexCell> GetNeighbors(IHexCell center);

        Vector3 PerformIntersectionWithTerrainSurface(Vector3 xzPosition);

        bool TryPerformIntersectionWithTerrainSurface(Vector3 xzPosition, out Vector3 hitpoint);

        void ToggleUI(bool isVisible);

        #endregion

    }

}
