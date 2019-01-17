using System;
using System.Collections.Generic;

namespace Assets.Simulation.HexMap {

    public interface IHexPathfinder {

        #region methods

        List<IHexCell> GetShortestPathBetween(
            IHexCell start, IHexCell end, Func<IHexCell, IHexCell, float> costFunction
        );

        List<IHexCell> GetShortestPathBetween(
            IHexCell start, IHexCell end, Func<IHexCell, IHexCell, float> costFunction,
            IEnumerable<IHexCell> availableCells
        );

        List<IHexCell> GetShortestPathBetween(
            IHexCell start, IHexCell end, float maxCost, Func<IHexCell, IHexCell, float> costFunction,
            IEnumerable<IHexCell> availableCells
        );

        Dictionary<IHexCell, float> GetCostToAllCells(
            IHexCell start, Func<IHexCell, IHexCell, float> costFunction,
            IEnumerable<IHexCell> availableCells
        );

        Dictionary<IHexCell, float> GetAllCellsReachableIn(
            IHexCell start, float maxCost, Func<IHexCell, IHexCell, float> costFunction,
            IEnumerable<IHexCell> availableCells
        );

        #endregion

    }

}