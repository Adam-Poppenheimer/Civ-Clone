using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.Diplomacy {

    [CreateAssetMenu(menuName = "Civ Clone/Diplomacy/Config")]
    public class DiplomacyConfig : ScriptableObject, IDiplomacyConfig {

        #region instance fields and properties

        #region from IDiplomacyConfig

        public int TradeDuration {
            get { return _tradeDuration; }
        }
        [SerializeField] private int _tradeDuration = 0;

        #endregion

        #endregion

    }

}
