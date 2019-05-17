using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.Improvements;

namespace Assets.Simulation.Technology {

    [Serializable]
    public class ImprovementModificationData : IImprovementModificationData {

        #region instance fields and properties

        public IImprovementTemplate Template {
            get { return _template; }
        }
        [SerializeField] private ImprovementTemplate _template = null;

        public YieldSummary BonusYield {
            get { return _bonusYield; }
        }
        [SerializeField] private YieldSummary _bonusYield = YieldSummary.Empty;

        public bool RequiresFreshWater {
            get { return _requiresFreshWater; }
        }
        [SerializeField] private bool _requiresFreshWater = false;

        #endregion

    }

}
