using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;
using Assets.Simulation.Units.Combat;

namespace Assets.Simulation.Units.Promotions {

    public interface IPromotion {

        #region properties

        string name { get; }

        string Description { get; }

        Sprite Icon { get; }

        bool RestrictedByTerrains              { get; }
        IEnumerable<TerrainType> ValidTerrains { get; }

        bool RestrictedByShapes               { get; }
        IEnumerable<TerrainShape> ValidShapes { get; }

        bool RestrictedByFeatures                 { get; }
        IEnumerable<TerrainFeature> ValidFeatures { get; }

        bool RestrictedByAttackerTypes           { get; }
        IEnumerable<UnitType> ValidAttackerTypes { get; }

        bool RestrictedByDefenderTypes           { get; }
        IEnumerable<UnitType> ValidDefenderTypes { get; }

        bool RequiresFlatTerrain  { get; }
        bool RequiresRoughTerrain { get; }

        bool RestrictedByCombatType { get; }
        CombatType ValidCombatType  { get; }

        bool AppliesWhileAttacking { get; }
        bool AppliesWhileDefending { get; }

        float CombatModifier { get; }

        bool CanMoveAfterAttacking    { get; }
        bool CanAttackAfterAttacking  { get; }
        bool IgnoresAmphibiousPenalty { get; }

        bool IgnoresDefensiveTerrainBonuses { get; }

        float GoldRaidingPercentage { get; }

        bool IgnoresLOSWhenAttacking { get; }


        bool IgnoresTerrainCosts { get; }

        #endregion

    }

}
