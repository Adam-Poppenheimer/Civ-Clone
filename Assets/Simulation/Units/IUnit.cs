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

        int CurrentHitpoints { get; set; }

        int MaxHitpoints { get; }

        IEnumerable<ISpecialtyResourceDefinition> RequiredResources { get; }

        List<IHexCell> CurrentPath { get; set; }

        int VisionRange { get; }

        bool CanAttack { get; set; }

        bool IsReadyForRangedAttack { get; }

        IUnitTemplate Template { get; }

        bool IsSetUpToBombard       { get; }
        bool LockedIntoConstruction { get; }
        bool IsIdling               { get; }

        IEnumerable<IPromotion> Promotions { get; }

        IPromotionTree PromotionTree { get; }

        int Experience { get; set; }
        int Level      { get; set; }

        bool IsWounded { get; }
        
        #endregion

        #region methods

        void PerformMovement();
        void PerformMovement(bool ignoreMoveCosts);

        bool CanRelocate(IHexCell newLocation);
        void Relocate   (IHexCell newLocation);

        void SetUpToBombard();
        void LockIntoConstruction();
        void BeginIdling();

        void Destroy();

        #endregion

    }

}
