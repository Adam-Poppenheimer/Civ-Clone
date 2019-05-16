using System;
using System.Collections.Generic;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public delegate int CrawlingWeightFunction(
        IHexCell cell, IHexCell seed, IEnumerable<IHexCell> acceptedCells
    );

    public interface IGridTraversalLogic {

        #region methods

        IEnumerator<IHexCell> GetCrawlingEnumerator(
            IHexCell seed, IEnumerable<IHexCell> unassignedCells,
            IEnumerable<IHexCell> acceptedCells,
            CrawlingWeightFunction weightFunction
        );

        #endregion

    }
}