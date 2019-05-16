using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.MapGeneration {

    [Serializable]
    public struct RegionBalanceStrategyData {

        #region instance fields and properties

        public string BalanceStrategy {
            get { return _balanceStrategy; }
        }
        [SerializeField] private string _balanceStrategy;

        public int Weight {
            get { return _weight; }
        }
        [SerializeField] private int _weight;

        #endregion

    }

}
