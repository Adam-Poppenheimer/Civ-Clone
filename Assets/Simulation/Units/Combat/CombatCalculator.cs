using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

namespace Assets.Simulation.Units.Combat {

    public class CombatCalculator : ICombatCalculator {

        #region instance fields and properties

        private IUnitConfig UnitConfig;

        #endregion

        #region constructors

        [Inject]
        public CombatCalculator(IUnitConfig unitConfig) {
            UnitConfig = unitConfig;
        }

        #endregion

        #region instance methods

        #region from ICombatCalculator

        public UniRx.Tuple<int, int> CalculateCombat(IUnit attacker, IUnit defender, CombatInfo combatInfo) {
            float attackerBaseStrength = combatInfo.CombatType == CombatType.Melee ? attacker.CombatStrength : attacker.RangedAttackStrength;

            float attackerStrength = attackerBaseStrength    * (1f + combatInfo.AttackerCombatModifier);
            float defenderStrength = defender.CombatStrength * (1f + combatInfo.DefenderCombatModifier);

            int attackerDamage = 0, defenderDamage = 0;

            if(attackerStrength == 0) {
                defenderDamage = attacker.CurrentHitpoints;

            }else if(defenderStrength == 0) {
                attackerDamage = defender.CurrentHitpoints;

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

            return new UniRx.Tuple<int, int>(
                combatInfo.CombatType == CombatType.Melee ? Math.Min(defenderDamage, attacker.CurrentHitpoints) : 0,
                Math.Min(attackerDamage, defender.CurrentHitpoints)
            );
        }

        #endregion

        #endregion
        
    }

}
