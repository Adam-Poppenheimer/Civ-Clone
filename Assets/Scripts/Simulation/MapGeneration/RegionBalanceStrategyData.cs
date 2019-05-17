using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.MapGeneration {

    [Serializable]
    public class RegionBalanceStrategyData {

        #region instance fields and properties

        public string BalanceStrategy {
            get { return _balanceStrategy; }
        }
        [SerializeField] private string _balanceStrategy = null;

        public int Weight {
            get { return _weight; }
        }
        [SerializeField] private int _weight = 0;

        #endregion

    }

}
