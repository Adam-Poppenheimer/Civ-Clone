using System;
using System.Collections.Generic;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public interface IGridTraversalLogic {

        #region methods

        IEnumerator<IHexCell> GetCrawlingEnumerator(
            IHexCell seed, IEnumerable<IHexCell> unassignedCells,
            Func<IHexCell, IHexCell, int> weightFunction
        );

        #endregion

    }
}