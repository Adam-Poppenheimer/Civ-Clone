using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.HexMap {

    public interface IHexGrid {

        #region properties

        ReadOnlyCollection<IHexCell> Tiles { get; }

        #endregion

        #region methods

        bool     HasCellAtCoordinates(HexCoordinates coords);
        IHexCell GetCellAtCoordinates(HexCoordinates coords);

        bool     HasNeighbor(IHexCell center, HexDirection direction);
        IHexCell GetNeighbor(IHexCell center, HexDirection direction);

        int GetDistance(IHexCell tileOne, IHexCell tileTwo);

        List<IHexCell> GetCellsInRadius(IHexCell center, int radius);

        List<IHexCell> GetCellsInLine(IHexCell start, IHexCell end);

        List<IHexCell> GetCellsInRing(IHexCell center, int radius);

        List<IHexCell> GetShortestPathBetween(IHexCell start, IHexCell end);

        List<IHexCell> GetShortestPathBetween(IHexCell start, IHexCell end, Func<IHexCell, int> costFunction);

        List<IHexCell> GetNeighbors(IHexCell center);

        void PaintCellTerrain(Vector3 position, TerrainType terrain);

        void PaintCellShape(Vector3 position, TerrainShape shape);

        void PaintCellFeature(Vector3 position, TerrainFeature feature);

        #endregion

    }

}
