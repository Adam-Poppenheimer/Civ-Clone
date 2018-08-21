using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public delegate bool SeedFilter(
        IHexCell cell, IEnumerable<IHexCell> landCells, IEnumerable<IHexCell> waterCell
    );

    public class TerrainData {

        #region instance fields and properties

        public readonly int Percentage;

        public readonly int SeedCount;

        public readonly CrawlingWeightFunction CrawlingWeightFunction;

        public readonly SeedFilter SeedFilter;

        public readonly Func<IHexCell, int> SeedWeightFunction;

        #endregion

        #region constructors

        public TerrainData(
            int percentage, int seedCount, CrawlingWeightFunction weightFunction,
            SeedFilter seedFilter, Func<IHexCell, int> seedWeightFunction
        ) {
            Percentage             = percentage;
            SeedCount              = seedCount;
            CrawlingWeightFunction = weightFunction;
            SeedFilter             = seedFilter;
            SeedWeightFunction     = seedWeightFunction;
        }

        #endregion

    }

}
