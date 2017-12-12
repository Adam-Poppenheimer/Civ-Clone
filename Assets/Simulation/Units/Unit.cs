using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.Units {

    public class Unit : MonoBehaviour, IUnit {

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

        private IUnitConfig Config;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(IUnitConfig config) {
            Config = config;
        }

        #region from IUnit



        #endregion

        #endregion
        
    }

}
