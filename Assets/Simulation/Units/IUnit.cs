using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;
using Assets.Simulation.SpecialtyResources;
using Assets.Simulation.Units.Abilities;

namespace Assets.Simulation.Units {

    public interface IUnit {

        #region properties

        string Name { get; }

        float MaxMovement { get; }

        float CurrentMovement { get; set; }

        UnitType Type { get; }

        bool IsAquatic { get; }

        IEnumerable<IAbilityDefinition> Abilities { get; }

        int AttackRange { get; }

        int CombatStrength { get; }

        int RangedAttackStrength { get; }

        int Hitpoints { get; set; }

        int MaxHitpoints { get; }

        IEnumerable<ISpecialtyResourceDefinition> RequiredResources { get; }

        List<IHexCell> CurrentPath { get; set; }

        int VisionRange { get; }

        bool HasAttacked { get; set; }

        IUnitTemplate Template { get; }

        GameObject gameObject { get; }

        Animator Animator { get; }

        bool PermittedToBombard { get; }
        
        #endregion

        #region methods

        void PerformMovement();

        #endregion

    }

}
