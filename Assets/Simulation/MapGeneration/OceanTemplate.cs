using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.MapGeneration {

    [CreateAssetMenu(menuName = "Civ Clone/Ocean Generation Template")]
    public class OceanTemplate : ScriptableObject, IOceanTemplate {

        #region instance fields and properties

        #region from IOceanGenerationTemplate

        public int DeepOceanLandPercentage {
            get { return _deepOceanLandPercentage; }
        }
        [SerializeField] private int _deepOceanLandPercentage;

        public int RegionSubdivisionXStep {
            get { return _regionSubdivisionXStep; }
        }
        [SerializeField] private int _regionSubdivisionXStep;

        public int RegionSubdivisionZStep {
            get { return _regionSubdivisionZStep; }
        }
        [SerializeField] private int _regionSubdivisionZStep;

        public IYieldAndResourcesTemplate ArchipelagoResources {
            get { return _archipelagoResources; }
        }
        [SerializeField] private YieldAndResourcesTemplate _archipelagoResources;

        #endregion

        #endregion

    }

}
