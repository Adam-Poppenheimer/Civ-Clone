using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Diplomacy;

namespace Assets.Simulation.Units.Combat {

    public class CombatExecuter : ICombatExecuter {

        #region instance fields and properties

        private IUnitPositionCanon                            UnitPositionCanon;
        private IHexGrid                                      Grid;
        private IUnitLineOfSightLogic                         UnitLineOfSightLogic;
        private ICombatInfoLogic                              CombatInfoLogic;
        private IUnitConfig                                   UnitConfig;
        private UnitSignals                                   UnitSignals;
        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private IWarCanon                                     WarCanon;

        #endregion

        #region constructors

        [Inject]
        public CombatExecuter(
            IUnitPositionCanon unitPositionCanon, IHexGrid grid, IUnitLineOfSightLogic unitLineOfSightLogic,
            ICombatInfoLogic combatModifierLogic, IUnitConfig unitConfig, UnitSignals unitSignals,
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon, IWarCanon warCanon
        ){
            UnitPositionCanon    = unitPositionCanon;
            Grid                 = grid;
            UnitLineOfSightLogic = unitLineOfSightLogic;
            CombatInfoLogic      = combatModifierLogic;
            UnitConfig           = unitConfig;
            UnitSignals          = unitSignals;
            UnitPossessionCanon  = unitPossessionCanon;
            WarCanon             = warCanon;
        }

        #endregion

        #region instance methods

        #region from ICombatExecuter

        public bool CanPerformMeleeAttack(IUnit attacker, IUnit defender) {
            if(attacker == null) {
                throw new ArgumentNullException("attacker");
            }
            if(defender == null) {
                throw new ArgumentNullException("defender");
            }

            if(attacker == defender) {
                return false;
            }

            var attackerOwner = UnitPossessionCanon.GetOwnerOfPossession(attacker);
            var defenderOwner = UnitPossessionCanon.GetOwnerOfPossession(defender);

            if(attackerOwner == defenderOwner) {
                return false;
            }

            if(!WarCanon.AreAtWar(attackerOwner, defenderOwner)) {
                return false;
            }

            var attackerLocation = UnitPositionCanon.GetOwnerOfPossession(attacker);
            var defenderLocation = UnitPositionCanon.GetOwnerOfPossession(defender);

            if(Grid.GetDistance(attackerLocation, defenderLocation) > 1) {
                return false;
            }

            if(HexMetrics.GetEdgeType(attackerLocation, defenderLocation) == HexEdgeType.Cliff) {
                return false;
            }

            if(!UnitPositionCanon.CanPlaceUnitAtLocation(attacker, defenderLocation, true)) {
                return false;
            }

            if(attacker.CurrentMovement <= 0 || !attacker.CanAttack) {
                return false;
            }

            if(!UnitLineOfSightLogic.GetCellsVisibleToUnit(attacker).Contains(defenderLocation)) {
                return false;
            }

            if(attacker.CombatStrength <= 0) {
                return false;
            }

            return true;
        }

        public bool CanPerformRangedAttack(IUnit attacker, IUnit defender) {
            if(attacker == null) {
                throw new ArgumentNullException("attacker");
            }
            if(defender == null) {
                throw new ArgumentNullException("defender");
            }

            if(attacker == defender) {
                return false;
            }

            var attackerOwner = UnitPossessionCanon.GetOwnerOfPossession(attacker);
            var defenderOwner = UnitPossessionCanon.GetOwnerOfPossession(defender);

            if(attackerOwner == defenderOwner) {
                return false;
            }

            if(!WarCanon.AreAtWar(attackerOwner, defenderOwner)) {
                return false;
            }

            var attackerLocation = UnitPositionCanon.GetOwnerOfPossession(attacker);
            var defenderLocation = UnitPositionCanon.GetOwnerOfPossession(defender);

            if(Grid.GetDistance(attackerLocation, defenderLocation) > attacker.AttackRange) {
                return false;
            }

            if(attacker.CurrentMovement <= 0 || !attacker.CanAttack) {
                return false;
            }

            if( !attacker.Template.IgnoresLineOfSight &&
                !UnitLineOfSightLogic.GetCellsVisibleToUnit(attacker).Contains(defenderLocation)
            ){
                return false;
            }

            if(attacker.RangedAttackStrength <= 0) {
                return false;
            }

            return true;
        }

        public void PerformMeleeAttack(IUnit attacker, IUnit defender) {
            if(!CanPerformMeleeAttack(attacker, defender)) {
                throw new InvalidOperationException("CanPerformMeleeCombat must return true");
            }

            int attackerStartingHealth = attacker.Hitpoints;
            int defenderStartingHealth = defender.Hitpoints;

            var defenderLocation = UnitPositionCanon.GetOwnerOfPossession(defender);

            var combatInfo = CombatInfoLogic.GetMeleeAttackInfo(attacker, defender, defenderLocation);

            PerformCombat(attacker, defender, combatInfo);

            UnitSignals.MeleeCombatWithUnitSignal.OnNext(
                new UnitCombatResults(
                    attacker, defender,
                    attackerStartingHealth - attacker.Hitpoints,
                    defenderStartingHealth - defender.Hitpoints
                )
            );
        }

        public void PerformRangedAttack(IUnit attacker, IUnit defender) {
            if(!CanPerformRangedAttack(attacker, defender)) {
                throw new InvalidOperationException("CanPerformRangedCombat must return true");
            }

            int attackerStartingHealth = attacker.Hitpoints;
            int defenderStartingHealth = defender.Hitpoints;

            var defenderLocation = UnitPositionCanon.GetOwnerOfPossession(defender);

            var combatInfo = CombatInfoLogic.GetRangedAttackInfo(attacker, defender, defenderLocation);

            PerformCombat(attacker, defender, combatInfo);

            UnitSignals.RangedCombatWithUnitSignal.OnNext(
                new UnitCombatResults(
                    attacker, defender,
                    attackerStartingHealth - attacker.Hitpoints,
                    defenderStartingHealth - defender.Hitpoints
                )
            );
        }

        public UnitCombatResults EstimateMeleeAttackResults(IUnit attacker, IUnit defender) {
            var defenderLocation = UnitPositionCanon.GetOwnerOfPossession(defender);

            var combatInfo = CombatInfoLogic.GetMeleeAttackInfo(attacker, defender, defenderLocation);

            Tuple<int, int> results = CalculateCombat(attacker, defender, combatInfo);

            return new UnitCombatResults(attacker, defender, results.Item1, results.Item2);
        }

        public UnitCombatResults EstimateRangedAttackResults(IUnit attacker, IUnit defender) {
            var defenderLocation = UnitPositionCanon.GetOwnerOfPossession(defender);

            var combatInfo = CombatInfoLogic.GetRangedAttackInfo(attacker, defender, defenderLocation);

            Tuple<int, int> results = CalculateCombat(attacker, defender, combatInfo);

            return new UnitCombatResults(attacker, defender, results.Item1, results.Item2);
        }

        #endregion

        private void PerformCombat(
            IUnit attacker, IUnit defender, CombatInfo combatInfo
        ) {
            if(combatInfo.AttackerCanMoveAfterAttacking) {
                attacker.CurrentMovement--;
            }else {
                attacker.CurrentMovement = 0;
            }

            Tuple<int, int> results = CalculateCombat(attacker, defender, combatInfo);

            attacker.Hitpoints -= results.Item1;
            defender.Hitpoints -= results.Item2;

            if(combatInfo.AttackerCanAttackAfterAttacking) {
                attacker.CanAttack = true;
            }else {
                attacker.CanAttack = false;
            }
        }

        private Tuple<int, int> CalculateCombat(
            IUnit attacker, IUnit defender, CombatInfo combatInfo
        ){
            float attackerBaseStrength = combatInfo.CombatType == CombatType.Melee ? attacker.CombatStrength : attacker.RangedAttackStrength;

            float attackerStrength = attackerBaseStrength    * (1f + combatInfo.AttackerCombatModifier);
            float defenderStrength = defender.CombatStrength * (1f + combatInfo.DefenderCombatModifier);

            int attackerDamage = 0, defenderDamage = 0;

            if(attackerStrength == 0) {
                defenderDamage = attacker.Hitpoints;

            }else if(defenderStrength == 0) {
                attackerDamage = defender.Hitpoints;

            }else {
                float attackerDefenderRatio = attackerStrength / defenderStrength;
                float defenderAttackerRatio = defenderStrength / attackerStrength;

                attackerDamage = Mathf.RoundToInt(
                    attackerDefenderRatio * UnitConfig.CombatBaseDamage
                );

                defenderDamage = Mathf.RoundToInt(
                    defenderAttackerRatio * UnitConfig.CombatBaseDamage
                );
            }

            return new Tuple<int, int>(
                combatInfo.CombatType == CombatType.Melee ? Math.Min(defenderDamage, attacker.Hitpoints) : 0,
                Math.Min(attackerDamage, defender.Hitpoints)
            );
        }

        #endregion

    }

}
