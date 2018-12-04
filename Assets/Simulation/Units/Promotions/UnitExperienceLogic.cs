using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.Units.Combat;

namespace Assets.Simulation.Units.Promotions {

    public class UnitExperienceLogic : IUnitExperienceLogic {

        #region instance fields and properties

        private IUnitConfig    Config;
        private IUnitModifiers UnitModifiers;

        #endregion

        #region constructors

        [Inject]
        public UnitExperienceLogic(
            IUnitConfig config, IUnitModifiers unitModifiers, UnitSignals unitSignals
        ) {
            Config        = config;
            UnitModifiers = unitModifiers;
            
            unitSignals.MeleeCombatWithUnitSignal .Subscribe(OnMeleeCombat);
            unitSignals.RangedCombatWithUnitSignal.Subscribe(OnRangedCombat);
        }

        #endregion

        #region instance methods

        #region from IUnitExperienceLogic

        public int GetExperienceForNextLevelOnUnit(IUnit unit) {
            if(unit.Level < Config.MaxLevel) {
                return unit.Level * Config.NextLevelExperienceCoefficient;
            }else {
                return int.MaxValue;
            }
        }

        #endregion

        private void OnMeleeCombat(UnitCombatResults combatResults) {
            var attacker = combatResults.Attacker;
            var defender = combatResults.Defender;

            if(defender.Type == UnitType.Civilian) {
                return;
            }

            if(attacker.Type != UnitType.City && attacker.CurrentHitpoints > 0) {
                attacker.Experience += Mathf.RoundToInt( 
                    Config.MeleeAttackerExperience * UnitModifiers.ExperienceGain.GetValueForUnit(attacker)
                );
            }

            if(defender.Type != UnitType.City && defender.CurrentHitpoints > 0) {
                defender.Experience += Mathf.RoundToInt(
                    Config.MeleeDefenderExperience * UnitModifiers.ExperienceGain.GetValueForUnit(defender)
                );
            }
        }

        private void OnRangedCombat(UnitCombatResults combatResults) {
            var attacker = combatResults.Attacker;
            var defender = combatResults.Defender;

            if(defender.Type == UnitType.Civilian) {
                return;
            }

            if(attacker.Type != UnitType.City && attacker.CurrentHitpoints > 0) {
                if(defender.Type != UnitType.City || combatResults.DamageToDefender > 0) {
                    attacker.Experience += Mathf.RoundToInt(
                        Config.RangedAttackerExperience * UnitModifiers.ExperienceGain.GetValueForUnit(attacker)
                    );
                }
            }

            if(defender.Type != UnitType.City && defender.CurrentHitpoints > 0) {
                defender.Experience += Mathf.RoundToInt(
                    Config.RangedDefenderExperience * UnitModifiers.ExperienceGain.GetValueForUnit(defender)
                );
            }
        }

        #endregion
        
    }

}
