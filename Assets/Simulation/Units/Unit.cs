using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.Units {

    public class Unit : MonoBehaviour, IUnit, IPointerClickHandler {

        #region instance fields and properties

        #region from IUnit

        public IUnitTemplate Template { get; set; }

        #endregion

        public int Health {
            get { return _health; }
            set {
                _health = value.Clamp(0, Config.MaxHealth);
            }
        }
        private int _health;

        public int CurrentMovement { get; set; }

        private IUnitConfig Config;

        private UnitSignals Signals;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(IUnitConfig config, UnitSignals signals) {
            Config = config;
            Signals = signals;
        }

        #region EventSystem handler implementations

        public void OnPointerClick(PointerEventData eventData) {
            Signals.UnitClickedSignal.OnNext(this);
        }

        #endregion

        #region from IUnit



        #endregion

        #endregion

    }

}
