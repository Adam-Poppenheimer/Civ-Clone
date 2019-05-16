using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.Units.Abilities;
using Assets.Simulation.Units.Promotions;
using Assets.Simulation.MapResources;

namespace Assets.Simulation.Units {

    public interface IUnitTemplate {

        #region properties

        string name { get; }

        string Description { get; }

        Sprite Image { get; }

        Sprite Icon { get; }

        GameObject DisplayPrefab { get; }

        int ProductionCost { get; }

        int MaxMovement { get; }

        UnitType Type { get; }

        IEnumerable<IAbilityDefinition> Abilities { get; }

        int AttackRange { get; }

        int CombatStrength { get; }

        int RangedAttackStrength { get; }

        int MaxHitpoints { get; }

        IEnumerable<IResourceDefinition> RequiredResources { get; }

        bool MustSetUpToBombard { get; }

        int VisionRange { get; }

        bool IgnoresLineOfSight { get; }

        IEnumerable<IPromotion> StartingPromotions { get; }

        IPromotionTreeTemplate PromotionTreeData { get; }

        IUnitMovementSummary MovementSummary { get; }

        #endregion

    }

}
