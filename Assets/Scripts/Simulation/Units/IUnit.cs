using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;
using Assets.Simulation.Units.Abilities;
using Assets.Simulation.Units.Promotions;
using Assets.Simulation.Units.Combat;

namespace Assets.Simulation.Units {

    public interface IUnit {

        #region properties

        string Name { get; }

        float MaxMovement { get; }

        float CurrentMovement { get; set; }

        UnitType Type { get; }

        IEnumerable<IAbilityDefinition> Abilities { get; }

        int AttackRange { get; }

        int CombatStrength { get; }

        int RangedAttackStrength { get; }

        int CurrentHitpoints { get; set; }

        int MaxHitpoints { get; }

        IEnumerable<IResourceDefinition> RequiredResources { get; }

        List<IHexCell> CurrentPath { get; set; }

        int VisionRange { get; }

        bool CanAttack { get; set; }

        bool IsReadyForRangedAttack { get; }

        IUnitTemplate Template { get; }

        bool IsSetUpToBombard       { get; }
        bool LockedIntoConstruction { get; }
        bool IsIdling               { get; }
        bool IsFortified            { get; }
        bool IsMoving               { get; }

        IPromotionTree PromotionTree { get; }

        int Experience { get; set; }
        int Level      { get; set; }

        bool IsWounded { get; }

        IUnitMovementSummary MovementSummary { get; }
        IUnitCombatSummary   CombatSummary   { get; }
        
        #endregion

        #region methods

        void PerformMovement();
        void PerformMovement(bool ignoreMoveCosts);
        void PerformMovement(bool ignoreMoveCosts, Action postMovementCallback);

        bool CanRelocate(IHexCell newLocation);
        void Relocate   (IHexCell newLocation);

        void SetUpToBombard();
        void LockIntoConstruction();
        void BeginIdling();
        void BeginFortifying();

        void Destroy();

        #endregion

    }

}
