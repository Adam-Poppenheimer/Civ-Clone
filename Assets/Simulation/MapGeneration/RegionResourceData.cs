using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.MapResources;

namespace Assets.Simulation.MapGeneration {

    [Serializable]
    public struct RegionResourceData {

        #region instance fields and properties

        public IResourceDefinition Resource {
            get { return _resource; }
        }
        [SerializeField] private ResourceDefinition _resource;

        public int Weight {
            get { return _weight; }
        }
        [SerializeField] private int _weight;

        #endregion

    }

}
