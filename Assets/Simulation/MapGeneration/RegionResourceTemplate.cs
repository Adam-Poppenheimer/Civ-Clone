using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.MapGeneration {

    [CreateAssetMenu(menuName = "Civ Clone/Region Resource Template")]
    public class RegionResourceTemplate : ScriptableObject, IRegionResourceTemplate {

        #region instance fields and properties

        #region from IRegionResourceTemplate

        public float StrategicNodesPerCell  {
            get {
                return _strategicNodesPerCell;
            }
        }
        [SerializeField] private float _strategicNodesPerCell;

        public float StrategicCopiesPerCell {
            get {
                return _strategicCopiesPerCell;
            }
        }
        [SerializeField] private float _strategicCopiesPerCell;

        #endregion

        #endregion
        
    }

}
