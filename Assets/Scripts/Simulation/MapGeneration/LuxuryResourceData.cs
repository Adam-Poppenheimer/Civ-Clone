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

        public bool ConstrainedToStarting {
            get { return StartingCount > 0 && OtherCount <= 0; }
        }

        public bool ConstrainedToOthers {
            get { return OtherCount > 0 && StartingCount <= 0; }
        }

        #endregion

        #region constructors

        public LuxuryResourceData(int startingCount, int otherCount) {
            _startingCount = startingCount;
            _otherCount    = otherCount;
        }

        #endregion

    }

}
