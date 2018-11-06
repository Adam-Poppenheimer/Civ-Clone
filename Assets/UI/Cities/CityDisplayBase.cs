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

        #region instance methods

        #region Unity messages

        private void OnEnable() {
            DoOnEnable();
        }

        private void OnDisable() {
            DoOnDisable();
        }

        protected virtual void DoOnEnable () { }
        protected virtual void DoOnDisable() { }

        #endregion

        #endregion

    }

}
