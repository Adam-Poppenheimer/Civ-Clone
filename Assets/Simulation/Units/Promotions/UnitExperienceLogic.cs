using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.Units.Combat;

namespace Assets.Simulation.Units.Promotions {

    public class UnitExperienceLogic : IUnitExperienceLogic {

        #region instance fields and properties

        private IUnitConfig Config;

        #endregion

        #region constructors

        [Inject]
        public UnitExperienceLogic(IUnitConfig config, UnitSignals unitSignals) {
            Config = config;
            
            unitSignals.MeleeCombatWithUnitSignal .Subscribe(OnMeleeCombat);
            unitSignals.RangedCombatWithUnitSignal.Subscribe(OnRangedCombat);
        }

        #endregion

        #region instance methods

        #region from IExperienceForNextLevelLogic

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
                attacker.Experience += Config.MeleeAttackerExperience;
            }

            if(defender.Type != UnitType.City && defender.CurrentHitpoints > 0) {
                defender.Experience += Config.MeleeDefenderExperience;
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
                    attacker.Experience += Config.RangedAttackerExperience;
                }
            }

            if(defender.Type != UnitType.City && defender.CurrentHitpoints > 0) {
                defender.Experience += Config.RangedDefenderExperience;
            }
        }

        #endregion
        
    }

}
