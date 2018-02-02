using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.SpecialtyResources {

    public class ResourceNode : MonoBehaviour, IResourceNode {

        #region instance fields and properties

        #region from IResourceNode

        public int Copies { get; set; }

        public ISpecialtyResourceDefinition Resource { get; set; }

        #endregion

        #endregion
        
    }

}
