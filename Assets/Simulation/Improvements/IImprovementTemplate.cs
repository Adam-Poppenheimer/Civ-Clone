using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Improvements {

    public interface IImprovementTemplate {

        #region properties

        string name { get; }

        IEnumerable<TerrainType> ValidTerrains { get; }

        IEnumerable<TerrainFeature> ValidFeatures { get; }

        ResourceSummary BonusYieldNormal { get; }

        bool RequiresAdjacentUpwardCliff { get; }

        bool ClearsForestsWhenBuilt { get; }

        float DefensiveBonus { get; }

        Transform AppearancePrefab { get; }

        #endregion

    }

}
