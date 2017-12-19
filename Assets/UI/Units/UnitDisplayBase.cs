using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.Units;
using Assets.Simulation.GameMap;
using Assets.Simulation.Units.Abilities;

namespace Assets.UI.Units {

    public class UnitDisplayBase : DisplayBase<IUnit> {

        #region instance fields and properties

        private UnitSignals Signals;

        private IDisposable UnitLocationChangedSubscription;

        private IDisposable UnitActivatedAbilitySubscription;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(UnitSignals signals) {
            Signals = signals;
        }

        #region Unity message methods

        public void OnEnable() {
            UnitLocationChangedSubscription  = Signals.UnitLocationChangedSignal .Subscribe(OnUnitLocationChangedFired);
            UnitActivatedAbilitySubscription = Signals.UnitActivatedAbilitySignal.Subscribe(OnUnitActivatedAbilityFired);
        }

        public void OnDisable() {
            UnitLocationChangedSubscription .Dispose();
            UnitActivatedAbilitySubscription.Dispose();
        }

        #endregion

        private void OnUnitLocationChangedFired(Tuple<IUnit, IMapTile> dataTuple) {
            if(dataTuple.Item1 == ObjectToDisplay) {
                Refresh();
            }
        }

        private void OnUnitActivatedAbilityFired(Tuple<IUnit, IUnitAbilityDefinition> dataTuple) {
            if(dataTuple.Item1 == ObjectToDisplay) {
                Refresh();
            }
        }

        #endregion

    }

}
