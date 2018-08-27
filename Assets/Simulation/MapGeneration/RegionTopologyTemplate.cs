using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Simulation.MapResources;
using UnityCustomUtilities.Extensions;
using UnityEngine;

namespace Assets.Simulation.MapGeneration {

    [CreateAssetMenu(menuName = "Civ Clone/Region Topology Template")]
    public class RegionTopologyTemplate : ScriptableObject, IRegionTopologyTemplate {

        #region instance fields and properties

        #region from IRegionTopologyTemplate

        public int HillsPercentage {
            get { return _hillsPercentage; }
        }
        [SerializeField] private int _hillsPercentage;

        public int MountainsPercentage {
            get { return _mountainsPercentage; }
        }
        [SerializeField] private int _mountainsPercentage;

        public IEnumerable<RegionBalanceStrategyData> BalanceStrategyWeights {
            get { return _balanceStrategyWeights; }
        }
        [SerializeField] private List<RegionBalanceStrategyData> _balanceStrategyWeights;

        public IEnumerable<RegionResourceData> ResourceWeights {
            get { return _resourceWeights; }
        }
        [SerializeField] private List<RegionResourceData> _resourceWeights;

        #endregion

        #endregion

    }

}
