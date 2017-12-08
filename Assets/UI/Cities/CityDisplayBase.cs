using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation.Core;

using Assets.Simulation.Cities;

namespace Assets.UI.Cities {

    public abstract class CityDisplayBase : DisplayBase<ICity> {

        #region instance fields and properties

        [Inject]
        private SlotDisplayClickedSignal SlotClickedSignal {
            get { return _slotClickedSignal; }
            set {
                _slotClickedSignal = value;
                _slotClickedSignal.Listen(OnDisplayClickedFired);
            }
        }
        private SlotDisplayClickedSignal _slotClickedSignal;

        #endregion

        #region instance methods

        private void OnDisplayClickedFired(IWorkerSlotDisplay slotDisplay) {
            Refresh();
        }

        #endregion

    }

}
