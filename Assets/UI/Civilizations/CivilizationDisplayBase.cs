using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.UI;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Core;

namespace Assets.UI.Civilizations {

    public class CivilizationDisplayBase : DisplayBase<ICivilization> {

        #region instance fields and properties

        private List<IDisposable> SignalSubscriptions = new List<IDisposable>();

        #endregion

        #region instance methods

        [Inject]
        private void InjectSignals(CoreSignals coreSignals) {            
            SignalSubscriptions.Add(coreSignals.TurnBeganSignal.Subscribe(OnTurnBegan));
        }

        #region Unity messages

        private void OnDestroy() {
            SignalSubscriptions.ForEach(subscription => subscription.Dispose());
        }

        #endregion

        private void OnTurnBegan(ICivilization civ) {
            if(isActiveAndEnabled) {
                ObjectToDisplay = civ;
                Refresh();
            }
        }

        #endregion

    }

}
