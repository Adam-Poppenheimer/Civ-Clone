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



        bool PermitsLandTraversal         { get; }
        bool PermitsShallowWaterTraversal { get; }
        bool PermitsDeepWaterTraversal    { get; }

        int BonusMovement { get; }
        int BonusVision   { get; }

        IEnumerable<CellTerrain>    TerrainsWithIgnoredCosts    { get; }
        IEnumerable<CellShape>      ShapesWithIgnoredCosts      { get; }
        IEnumerable<CellVegetation> VegetationsWithIgnoredCosts { get; }

        IEnumerable<CellShape>      ShapesConsumingFullMovement      { get; }
        IEnumerable<CellVegetation> VegetationsConsumingFullMovement { get; }



        bool CanMoveAfterAttacking    { get; }
        bool CanAttackAfterAttacking  { get; }
        bool IgnoresAmphibiousPenalty { get; }

        bool IgnoresDefensiveTerrainBonuses { get; }
        bool IgnoresLineOfSight             { get; }

        float GoldRaidingPercentage { get; }

        IEnumerable<ICombatModifier> ModifiersWhenAttacking { get; }
        IEnumerable<ICombatModifier> ModifiersWhenDefending { get; }

        IEnumerable<ICombatModifier> AuraModifiersWhenAttacking { get; }
        IEnumerable<ICombatModifier> AuraModifiersWhenDefending { get; }



        bool RequiresForeignTerritory { get; }

        bool HealsEveryTurn { get; }

        int BonusHealingToSelf     { get; }
        int BonusHealingToAdjacent { get; }

        int AlternativeNavalBaseHealing { get; }

        #endregion

    }

}
