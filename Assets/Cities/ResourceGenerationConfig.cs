using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Cities {

    [CreateAssetMenu(menuName = "Civ Clone/Resource Generation Config")]
    public class ResourceGenerationConfig : ScriptableObject, IResourceGenerationConfig {

        #region instance fields and properties

        #region from IResourceGenerationConfig

        public ResourceSummary UnemployedYield {
            get { return _unemployedYield; }
        }
        [SerializeField] private ResourceSummary _unemployedYield;

        #endregion

        #endregion

    }

}
