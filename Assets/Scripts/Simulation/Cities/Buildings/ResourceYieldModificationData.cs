using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Simulation.MapResources;
using UnityEngine;

namespace Assets.Simulation.Cities.Buildings {

    [Serializable]
    public class ResourceYieldModificationData : IResourceYieldModificationData {

        #region instance fields and properties

        #region from IResourceYieldModificationData

        public YieldSummary BonusYield {
            get { return _bonusYield; }
        }
        [SerializeField] private YieldSummary _bonusYield = YieldSummary.Empty;

        public IResourceDefinition Resource {
            get { return _resource; }
        }
        [SerializeField] private ResourceDefinition _resource = null;

        #endregion

        #endregion

    }

}
