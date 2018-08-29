using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.MapGeneration {

    [CreateAssetMenu(menuName = "Civ Clone/Civ Homeland Template")]
    public class CivHomelandTemplate : ScriptableObject, ICivHomelandTemplate {

        #region instance fields and properties

        #region from ICivHomelandTemplate

        public IRegionResourceTemplate StartingResources {
            get { return _startingResources; }
        }
        [SerializeField] private RegionResourceTemplate _startingResources;

        public IRegionResourceTemplate OtherResources {
            get { return _otherResources; }
        }
        [SerializeField] private RegionResourceTemplate _otherResources;

        public IEnumerable<LuxuryResourceData> LuxuryResourceData {
            get { return _luxuryResourceData; }
        }
        [SerializeField] private List<LuxuryResourceData> _luxuryResourceData;

        public int RegionCount {
            get { return _regionCount; }
        }
        [SerializeField, Range(2, 20)] private int _regionCount;

        #endregion

        #endregion
        
    }

}
