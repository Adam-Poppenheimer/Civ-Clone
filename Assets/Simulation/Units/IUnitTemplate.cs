using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.Units.Abilities;
using Assets.Simulation.Units.Promotions;
using Assets.Simulation.SpecialtyResources;

namespace Assets.Simulation.Units {

    public interface IUnitTemplate {

        #region properties

        string name { get; }

        string Description { get; }

        Sprite Image { get; }

        Sprite Icon { get; }

        GameObject Prefab { get; }

        int ProductionCost { get; }

        int MaxMovement { get; }

        UnitType Type { get; }

        bool IsAquatic { get; }

        IEnumerable<IAbilityDefinition> Abilities { get; }

        int AttackRange { get; }

        int CombatStrength { get; }

        int RangedAttackStrength { get; }

        int MaxHitpoints { get; }

        IEnumerable<ISpecialtyResourceDefinition> RequiredResources { get; }

        bool IsImmobile { get; }

        bool MustSetUpToBombard { get; }

        int VisionRange { get; }

        bool IgnoresLineOfSight { get; }

        IEnumerable<IPromotion> StartingPromotions { get; }

        IPromotionTreeTemplate PromotionTreeData { get; }

        #endregion

    }

}
