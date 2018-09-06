using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.MapGeneration {

    [CreateAssetMenu(menuName = "Civ Clone/Civ Homeland Template")]
    public class HomelandTemplate : ScriptableObject, IHomelandTemplate {

        #region instance fields and properties

        #region from ICivHomelandTemplate

        public IEnumerable<LuxuryResourceData> LuxuryResourceData {
            get { return _luxuryResourceData; }
        }
        [SerializeField] private List<LuxuryResourceData> _luxuryResourceData;

        public int RegionCount {
            get { return _regionCount; }
        }
        [SerializeField, Range(2, 20)] private int _regionCount;

        public IYieldAndResourcesTemplate YieldAndResources {
            get { return _yieldAndResources; }
        }
        [SerializeField] private YieldAndResourcesTemplate _yieldAndResources;

        #endregion

        #endregion
        
    }

}
