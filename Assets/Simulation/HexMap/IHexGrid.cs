using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.HexMap {

    public interface IHexGrid {

        #region properties

        ReadOnlyCollection<IHexCell> AllCells { get; }

        int ChunkCountX { get; }
        int ChunkCountZ { get; }

        #endregion

        #region methods

        void Build();
        void Clear();

        bool     HasCellAtCoordinates(HexCoordinates coords);
        IHexCell GetCellAtCoordinates(HexCoordinates coords);

        bool     HasCellAtLocation(Vector3 location);
        IHexCell GetCellAtLocation(Vector3 location);

        bool     HasNeighbor(IHexCell center, HexDirection direction);
        IHexCell GetNeighbor(IHexCell center, HexDirection direction);

        int GetDistance(IHexCell tileOne, IHexCell tileTwo);

        List<IHexCell> GetCellsInRadius(IHexCell center, int radius);

        List<IHexCell> GetCellsInLine(IHexCell start, IHexCell end);

        List<IHexCell> GetCellsInRing(IHexCell center, int radius);

        List<IHexCell> GetShortestPathBetween(IHexCell start, IHexCell end);

        List<IHexCell> GetShortestPathBetween(IHexCell start, IHexCell end, Func<IHexCell, IHexCell, int> costFunction);

        List<IHexCell> GetNeighbors(IHexCell center);

        void ToggleUI(bool isVisible);

        #endregion

    }

}
