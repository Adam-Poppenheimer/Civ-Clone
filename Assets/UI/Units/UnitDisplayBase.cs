using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.Units;
using Assets.Simulation.HexMap;
using Assets.Simulation.Units.Abilities;
using Assets.Simulation.Units.Combat;

namespace Assets.UI.Units {

    public class UnitDisplayBase : DisplayBase<IUnit> {

        #region instance fields and properties

        private UnitSignals Signals;

        private IDisposable UnitLocationChangedSubscription;
        private IDisposable UnitActivatedAbilitySubscription;
        private IDisposable CombatEventOccurredSubscription;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(UnitSignals signals) {
            Signals = signals;
        }

        #region Unity message methods

        public void OnEnable() {
            UnitLocationChangedSubscription  = Signals.EnteredLocationSignal    .Subscribe(OnUnitEnteredLocation);
            UnitActivatedAbilitySubscription = Signals.ActivatedAbilitySignal   .Subscribe(OnUnitActivatedAbility);
            CombatEventOccurredSubscription  = Signals.MeleeCombatWithUnitSignal.Subscribe(OnCombatEventOccurred);

            DoOnEnable();
        }

        public void OnDisable() {
            UnitLocationChangedSubscription .Dispose();
            UnitActivatedAbilitySubscription.Dispose();
            CombatEventOccurredSubscription .Dispose();

            DoOnDisable();
        }

        protected virtual void DoOnEnable () { }
        protected virtual void DoOnDisable() { }

        #endregion

        private void OnUnitEnteredLocation(Tuple<IUnit, IHexCell> dataTuple) {
            if(dataTuple.Item1 == ObjectToDisplay) {
                Refresh();
            }
        }

        private void OnUnitActivatedAbility(Tuple<IUnit, IAbilityDefinition> dataTuple) {
            if(dataTuple.Item1 == ObjectToDisplay) {
                Refresh();
            }
        }

        private void OnCombatEventOccurred(UnitCombatResults results) {
            if(results.Attacker == ObjectToDisplay || results.Defender == ObjectToDisplay) {
                Refresh();
            }
        }

        #endregion

    }

}
