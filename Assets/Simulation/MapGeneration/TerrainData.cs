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

        public readonly Func<IHexCell, IHexCell, int> WeightFunction;

        public readonly Predicate<IHexCell> ForceAdaptFilter;

        #endregion

        #region constructors

        public TerrainData(
            int percentage, int seedCount, Func<IHexCell, IHexCell, int> weightFunction,
            Predicate<IHexCell> forceAdaptFilter
        ) {
            Percentage       = percentage;
            SeedCount        = seedCount;
            WeightFunction   = weightFunction;
            ForceAdaptFilter = forceAdaptFilter;
        }

        #endregion

    }

}
