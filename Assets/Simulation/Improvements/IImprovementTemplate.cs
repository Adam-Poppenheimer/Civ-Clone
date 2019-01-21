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

        IEnumerable<CellTerrain> RestrictedToTerrains { get; }

        IEnumerable<CellVegetation> RestrictedToVegetations { get; }

        IEnumerable<CellShape> RestrictedToShapes { get; }

        YieldSummary BonusYieldNormal { get; }

        bool ClearsVegetationWhenBuilt { get; }

        float DefensiveBonus { get; }

        bool RequiresResourceToExtract { get; }

        Transform AppearancePrefab { get; }

        int TurnsToConstruct { get; }

        bool FreshWaterAlwaysEnables { get; }

        float AdjacentEnemyDamagePercentage { get; }

        bool OverridesTerrain { get; }

        int OverridingTerrainIndex { get; }

        #endregion

    }

}
