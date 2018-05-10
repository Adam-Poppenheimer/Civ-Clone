using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Cities.Buildings {

    public interface ICellYieldModificationData {

        #region properties

        CellPropertyType PropertyConsidered { get; }

        TerrainType      TerrainRequired    { get; }
        TerrainShape     ShapeRequired      { get; }
        TerrainFeature   FeatureRequired    { get; }

        bool MustBeUnderwater { get; }

        ResourceSummary BonusYield { get; }

        #endregion

    }

}
