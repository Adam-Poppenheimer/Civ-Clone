using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;
using Assets.Simulation.SpecialtyResources;
using Assets.Simulation.Units.Abilities;
using Assets.Simulation.Units.Promotions;

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

        bool CanAttack { get; set; }

        IUnitTemplate Template { get; }

        bool IsPermittedToBombard { get; }

        bool LockedIntoConstruction { get; }

        IEnumerable<IPromotion> Promotions { get; }
        
        #endregion

        #region methods

        void PerformMovement();
        void PerformMovement(bool ignoreMoveCosts);

        void SetUpToBombard();
        void LockIntoConstruction();

        void Destroy();
        void SetParent(Transform newParent, bool worldPositionStays);

        #endregion

    }

}
