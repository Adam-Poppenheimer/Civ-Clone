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

        bool RestrictedByOpponentTypes           { get; }
        IEnumerable<UnitType> ValidOpponentTypes { get; }

        bool RequiresFlatTerrain  { get; }
        bool RequiresRoughTerrain { get; }

        bool RestrictedByCombatType { get; }
        CombatType ValidCombatType  { get; }

        bool AppliesWhileAttacking { get; }
        bool AppliesWhileDefending { get; }

        bool RequiresForeignTerritory { get; }

        float CombatModifier { get; }

        bool CanMoveAfterAttacking    { get; }
        bool CanAttackAfterAttacking  { get; }
        bool IgnoresAmphibiousPenalty { get; }

        bool IgnoresDefensiveTerrainBonuses { get; }

        float GoldRaidingPercentage { get; }

        bool IgnoresLOSWhenAttacking { get; }

        bool RestrictedByOpponentWoundedState { get; }
        bool ValidOpponentWoundedState        { get; }


        bool IgnoresTerrainCosts { get; }


        bool HealsEveryTurn { get; }

        int BonusHealingToSelf     { get; }
        int BonusHealingToAdjacent { get; }

        int AlternativeNavalBaseHealing { get; }

        #endregion

    }

}
