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

        Sprite Icon { get; }

        IEnumerable<TerrainType> RestrictedToTerrains { get; }

        IEnumerable<TerrainFeature> RestrictedToFeatures { get; }

        IEnumerable<TerrainShape> RestrictedToShapes { get; }

        ResourceSummary BonusYieldNormal { get; }

        bool ClearsForestsWhenBuilt { get; }

        float DefensiveBonus { get; }

        bool RequiresResourceToExtract { get; }

        Transform AppearancePrefab { get; }

        #endregion

    }

}
