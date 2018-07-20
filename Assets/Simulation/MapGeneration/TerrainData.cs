using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public class TerrainData {

        #region instance fields and properties

        public readonly int Percentage;

        public readonly int SeedCount;

        public readonly CrawlingWeightFunction WeightFunction;

        public readonly Func<IHexCell, bool> SeedFilter;

        public readonly Predicate<IHexCell> ForceAdaptFilter;

        #endregion

        #region constructors

        public TerrainData(
            int percentage, int seedCount, CrawlingWeightFunction weightFunction,
            Func<IHexCell, bool> seedFilter, Predicate<IHexCell> forceAdaptFilter
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
