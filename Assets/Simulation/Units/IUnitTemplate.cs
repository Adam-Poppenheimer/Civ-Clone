using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.Units.Abilities;
using Assets.Simulation.SpecialtyResources;

namespace Assets.Simulation.Units {

    public interface IUnitTemplate {

        #region properties

        string Name { get; }

        string Description { get; }

        Sprite Image { get; }

        Sprite Icon { get; }

        int ProductionCost { get; }

        int MaxMovement { get; }

        UnitType Type { get; }

        IEnumerable<IAbilityDefinition> Abilities { get; }

        int AttackRange { get; }

        int CombatStrength { get; }

        int RangedAttackStrength { get; }

        IEnumerable<ISpecialtyResourceDefinition> RequiredResources { get; }

        bool BenefitsFromDefensiveTerrain { get; }

        

        #endregion

    }

}
