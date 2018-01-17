using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units.Combat {

    public class CombatExecuter : ICombatExecuter {

        #region instance fields and properties

        private IUnitPositionCanon UnitPositionCanon;

        private IHexGrid Grid;

        private ILineOfSightLogic LineOfSightLogic;

        private ICombatModifierLogic CombatModifierLogic;

        private IUnitConfig UnitConfig;

        #endregion

        #region constructors

        [Inject]
        public CombatExecuter(
            IUnitPositionCanon unitPositionCanon, IHexGrid grid, ILineOfSightLogic lineOfSightLogic,
            ICombatModifierLogic combatModifierLogic, IUnitConfig unitConfig
        ){
            UnitPositionCanon   = unitPositionCanon;
            Grid                = grid;
            LineOfSightLogic    = lineOfSightLogic;
            CombatModifierLogic = combatModifierLogic;
            UnitConfig          = unitConfig;
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

            var attackerLocation = UnitPositionCanon.GetOwnerOfPossession(attacker);
            var defenderLocation = UnitPositionCanon.GetOwnerOfPossession(defender);

            if(Grid.GetDistance(attackerLocation, defenderLocation) > 1) {
                return false;
            }

            if(HexMetrics.GetEdgeType(attackerLocation.Elevation, defenderLocation.Elevation) == HexEdgeType.Cliff) {
                return false;
            }

            if(!UnitPositionCanon.CanChangeOwnerOfPossession(attacker, defenderLocation)) {
                return false;
            }

            if(attacker.CurrentMovement <= 0) {
                return false;
            }

            if(!LineOfSightLogic.CanUnitSeeCell(attacker, defenderLocation)) {
                return false;
            }

            if(attacker.Template.CombatStrength <= 0) {
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

            var attackerLocation = UnitPositionCanon.GetOwnerOfPossession(attacker);
            var defenderLocation = UnitPositionCanon.GetOwnerOfPossession(defender);

            if(Grid.GetDistance(attackerLocation, defenderLocation) > attacker.Template.AttackRange) {
                return false;
            }

            if(attacker.CurrentMovement <= 0) {
                return false;
            }

            if(!LineOfSightLogic.CanUnitSeeCell(attacker, defenderLocation)) {
                return false;
            }

            if(attacker.Template.RangedAttackStrength <= 0) {
                return false;
            }

            return true;
        }

        public void PerformMeleeAttack(IUnit attacker, IUnit defender) {
            if(!CanPerformMeleeAttack(attacker, defender)) {
                throw new InvalidOperationException("CanPerformMeleeCombat must return true");
            }

            var defenderLocation = UnitPositionCanon.GetOwnerOfPossession(defender);

            var attackerModifier = CombatModifierLogic.GetMeleeOffensiveModifierAtLocation(attacker, defender, defenderLocation);
            var defenderModifier = CombatModifierLogic.GetMeleeDefensiveModifierAtLocation(attacker, defender, defenderLocation);

            var attackerStrength = attacker.Template.CombatStrength * (1f + attackerModifier);
            var defenderStrength = defender.Template.CombatStrength * (1f + defenderModifier);

            PerformCombat(attacker, attackerStrength, defender, defenderStrength, true);
        }

        public void PerformRangedAttack(IUnit attacker, IUnit defender) {
            if(!CanPerformRangedAttack(attacker, defender)) {
                throw new InvalidOperationException("CanPerformRangedCombat must return true");
            }

            var defenderLocation = UnitPositionCanon.GetOwnerOfPossession(defender);

            var attackerModifier = CombatModifierLogic.GetRangedOffensiveModifierAtLocation(attacker, defender, defenderLocation);
            var defenderModifier = CombatModifierLogic.GetRangedDefensiveModifierAtLocation(attacker, defender, defenderLocation);

            var attackerStrength = attacker.Template.RangedAttackStrength * (1f + attackerModifier);
            var defenderStrength = defender.Template.CombatStrength       * (1f + defenderModifier);

            PerformCombat(attacker, attackerStrength, defender, defenderStrength, false);
        }

        #endregion

        private void PerformCombat(
            IUnit attacker, float attackerStrength, IUnit defender, float defenderStrength,
            bool attackerReceivesDamage
        ){
            attacker.CurrentMovement = 0;

            if(attackerStrength == 0) {
                InflictDamage(attacker, UnitConfig.MaxHealth);

            }else if(defenderStrength == 0) {
                InflictDamage(defender, UnitConfig.MaxHealth);

            }else {
                float attackerDefenderRatio = attackerStrength / defenderStrength;
                float defenderAttackerRatio = defenderStrength / attackerStrength;

                int attackerDamage = Mathf.RoundToInt(
                    attackerDefenderRatio * UnitConfig.CombatBaseDamage
                );

                int defenderDamage = Mathf.RoundToInt(
                    defenderAttackerRatio * UnitConfig.CombatBaseDamage
                );

                InflictDamage(defender, attackerDamage);

                if(attackerReceivesDamage) {
                    InflictDamage(attacker, defenderDamage);
                }
            }
        }

        private void InflictDamage(IUnit unit, int damage) {
            unit.Health -= Math.Min(damage, unit.Health);
        }

        #endregion
        
    }

}
