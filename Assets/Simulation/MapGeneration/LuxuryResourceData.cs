using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.MapGeneration {

    [Serializable]
    public struct LuxuryResourceData {

        #region instance fields and properties

        public int StartingCount {
            get { return _startingCount; }
        }
        [SerializeField] private int _startingCount;

        public int OtherCount {
            get { return _otherCount; }
        }
        [SerializeField] private int _otherCount;

        #endregion

    }

}
