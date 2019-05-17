using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Simulation.MapResources;
using UnityCustomUtilities.Extensions;
using UnityEngine;

namespace Assets.Simulation.MapGeneration {

    [CreateAssetMenu(menuName = "Civ Clone/Map Generation/Region Topology Template")]
    public class RegionTopologyTemplate : ScriptableObject, IRegionTopologyTemplate {

        #region instance fields and properties

        #region from IRegionTopologyTemplate

        public int HillsPercentage {
            get { return _hillsPercentage; }
        }
        [SerializeField] private int _hillsPercentage = 0;

        public int MountainsPercentage {
            get { return _mountainsPercentage; }
        }
        [SerializeField] private int _mountainsPercentage = 0;

        public IEnumerable<RegionBalanceStrategyData> BalanceStrategyWeights {
            get { return _balanceStrategyWeights; }
        }
        [SerializeField] private List<RegionBalanceStrategyData> _balanceStrategyWeights = null;

        public IEnumerable<RegionResourceData> ResourceWeights {
            get { return _resourceWeights; }
        }
        [SerializeField] private List<RegionResourceData> _resourceWeights = null;

        #endregion

        #endregion

    }

}
