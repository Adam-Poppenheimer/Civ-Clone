using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.Core;

namespace Assets.Simulation.Units.Combat {

    public class UnitFortificationLogic : IUnitFortificationLogic {

        #region instance fields and properties

        private Dictionary<IUnit, float> FortificationBonusForUnit =
            new Dictionary<IUnit, float>();



        private IUnitConfig UnitConfig;
        private CoreSignals CoreSignals;

        #endregion

        #region constructors

        [Inject]
        public UnitFortificationLogic(IUnitConfig unitConfig, CoreSignals coreSignals) {
            UnitConfig  = unitConfig;
            
            coreSignals.RoundBegan.Subscribe(OnRoundBegan);
        }

        #endregion

        #region instance methods

        #region from IUnitFortificationLogic

        public bool GetFortificationStatusForUnit(IUnit unit) {
            return FortificationBonusForUnit.ContainsKey(unit);
        }

        public void SetFortificationStatusForUnit(IUnit unit, bool isFortified) {
            if(isFortified && !FortificationBonusForUnit.ContainsKey(unit)) {

                float startingFortifyBonus = unit.CombatSummary.BenefitsFromFortifications
                                           ? UnitConfig.FortificationBonusPerTurn
                                           : 0;

                FortificationBonusForUnit.Add(unit, Math.Min(UnitConfig.MaxFortificationBonus, startingFortifyBonus));

            }else {
                FortificationBonusForUnit.Remove(unit);
            }
        }

        public float GetFortificationModifierForUnit(IUnit unit) {
            float retval;

            FortificationBonusForUnit.TryGetValue(unit, out retval);

            return retval;
        }

        #endregion

        private void OnRoundBegan(int roundNumber) {
            foreach(var unit in FortificationBonusForUnit.Keys.ToArray()) {

                if(unit.CombatSummary.BenefitsFromFortifications) {
                    float fortifyBonus;
                    FortificationBonusForUnit.TryGetValue(unit, out fortifyBonus);

                    fortifyBonus = Math.Min(
                        fortifyBonus + UnitConfig.FortificationBonusPerTurn,
                        UnitConfig.MaxFortificationBonus
                    );

                    FortificationBonusForUnit[unit] = fortifyBonus;
                }

            }
        }

        #endregion

    }

}
