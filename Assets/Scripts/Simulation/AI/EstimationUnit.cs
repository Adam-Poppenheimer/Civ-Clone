using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Abilities;
using Assets.Simulation.Units.Combat;
using Assets.Simulation.Units.Promotions;

namespace Assets.Simulation.AI {

    public class EstimationUnit : IUnit {

        #region static fields and properties

        private static string EstimationErrorMessage = "This unit is an EstimationUnit, which does not implement this member. " +
            "EstimationUnits should not be treated as normal units.";

        #endregion

        #region instance fields and properties

        #region from IUnit

        public IEnumerable<IAbilityDefinition> Abilities {
            get { return Template.Abilities; }
        }

        public int AttackRange {
            get { return Template.AttackRange; }
        }

        public bool CanAttack { get; set; }

        public int CombatStrength {
            get { return Template.CombatStrength; }
        }

        public IUnitCombatSummary CombatSummary { get; private set; }

        public int CurrentHitpoints  { get; set; }
        public float CurrentMovement { get; set; }

        public List<IHexCell> CurrentPath { get; set; }

        public int Experience { get; set; }

        public bool IsFortified {
            get { return false; }
        }

        public bool IsIdling {
            get { return false; }
        }

        public bool IsMoving {
            get { return false; }
        }

        public bool IsReadyForRangedAttack {
            get { return true; }
        }

        public bool IsSetUpToBombard {
            get { return false; }
        }

        public bool IsWounded {
            get { return CurrentHitpoints < MaxHitpoints; }
        }

        public int Level { get; set; }

        public bool LockedIntoConstruction {
            get { return false; }
        }

        public int MaxHitpoints {
            get { return Template.MaxHitpoints; }
        }

        public float MaxMovement {
            get { return Template.MaxMovement; }
        }

        public IUnitMovementSummary MovementSummary {
            get { throw new NotImplementedException(EstimationErrorMessage); }
        }

        public string Name {
            get { return "EstimationUnit for template " + Template.name; }
        }

        public IPromotionTree PromotionTree {
            get { throw new NotImplementedException(EstimationErrorMessage); }
        }

        public int RangedAttackStrength {
            get { return Template.RangedAttackStrength; }
        }

        public IEnumerable<IResourceDefinition> RequiredResources {
            get { return Template.RequiredResources; }
        }

        public IUnitTemplate Template { get; private set; }

        public UnitType Type {
            get { return Template.Type; }
        }

        public int VisionRange {
            get { return Template.VisionRange; }
        }

        #endregion

        #endregion

        #region constructors

        public EstimationUnit(IUnitTemplate template) {
            CanAttack        = true;
            CurrentHitpoints = template.MaxHitpoints;
            Template         = template;

            CombatSummary = new UnitCombatSummary() {
                modifiersWhenAttacking = new List<ICombatModifier>(),
                modifiersWhenDefending = new List<ICombatModifier>()
            };
        }

        #endregion

        #region instance methods

        #region from IUnit

        public void BeginFortifying() {
            throw new NotImplementedException(EstimationErrorMessage);
        }

        public void BeginIdling() {
            throw new NotImplementedException(EstimationErrorMessage);
        }

        public bool CanRelocate(IHexCell newLocation) {
            throw new NotImplementedException(EstimationErrorMessage);
        }

        public void Destroy() {
            throw new NotImplementedException(EstimationErrorMessage);
        }

        public void LockIntoConstruction() {
            throw new NotImplementedException(EstimationErrorMessage);
        }

        public void PerformMovement() {
            throw new NotImplementedException(EstimationErrorMessage);
        }

        public void PerformMovement(bool ignoreMoveCosts) {
            throw new NotImplementedException(EstimationErrorMessage);
        }

        public void PerformMovement(bool ignoreMoveCosts, Action postMovementCallback) {
            throw new NotImplementedException(EstimationErrorMessage);
        }

        public void Relocate(IHexCell newLocation) {
            throw new NotImplementedException(EstimationErrorMessage);
        }

        public void SetUpToBombard() {
            throw new NotImplementedException(EstimationErrorMessage);
        }

        public void RefreshPosition() {
            throw new NotImplementedException(EstimationErrorMessage);
        }

        #endregion

        #endregion

    }

}
