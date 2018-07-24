using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public delegate bool SeedFilter(IHexCell cell, IEnumerable<IHexCell> oceanCells);

    public class TerrainData {

        #region instance fields and properties

        public readonly int Percentage;

        public readonly int SeedCount;

        public readonly CrawlingWeightFunction WeightFunction;

        public readonly SeedFilter SeedFilter;

        public readonly Predicate<IHexCell> ForceAdaptFilter;

        #endregion

        #region constructors

        public TerrainData(
            int percentage, int seedCount, CrawlingWeightFunction weightFunction,
            SeedFilter seedFilter, Predicate<IHexCell> forceAdaptFilter
        ) {
            Percentage       = percentage;
            SeedCount        = seedCount;
            WeightFunction   = weightFunction;
            SeedFilter       = seedFilter;
            ForceAdaptFilter = forceAdaptFilter;
        }

        #endregion

    }

}
