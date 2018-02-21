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

        int MaxMovement { get; }

        int CurrentMovement { get; set; }

        UnitType Type { get; }

        bool IsAquatic { get; }

        IEnumerable<IAbilityDefinition> Abilities { get; }

        int AttackRange { get; }

        int CombatStrength { get; }

        int RangedAttackStrength { get; }

        int Health { get; set; }

        int MaxHealth { get; }

        IEnumerable<ISpecialtyResourceDefinition> RequiredResources { get; }

        List<IHexCell> CurrentPath { get; set; }

        int VisionRange { get; }

        IUnitTemplate Template { get; }

        GameObject gameObject { get; }
        
        #endregion

        #region methods

        void PerformMovement();

        #endregion

    }

}
