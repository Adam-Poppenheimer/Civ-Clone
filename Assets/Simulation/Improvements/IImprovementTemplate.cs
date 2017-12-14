using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.GameMap;

namespace Assets.Simulation.Improvements {

    public interface IImprovementTemplate {

        #region properties

        string name { get; }

        float WorkToComplete { get; }

        IEnumerable<TerrainType> ValidTerrains { get; }

        IEnumerable<TerrainShape> ValidShapes { get; }

        IEnumerable<TerrainFeatureType> ValidFeatures { get; }

        ResourceSummary BonusYield { get; }

        #endregion

    }

}
