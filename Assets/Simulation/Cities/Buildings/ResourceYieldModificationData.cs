using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Simulation.SpecialtyResources;
using UnityEngine;

namespace Assets.Simulation.Cities.Buildings {

    [Serializable]
    public struct ResourceYieldModificationData : IResourceYieldModificationData {

        #region instance fields and properties

        #region from IResourceYieldModificationData

        public ResourceSummary BonusYield {
            get { return _bonusYield; }
        }
        [SerializeField] private ResourceSummary _bonusYield;

        public ISpecialtyResourceDefinition Resource {
            get { return _resource; }
        }
        [SerializeField] private SpecialtyResourceDefinition _resource;

        #endregion

        #endregion
        
    }

}
