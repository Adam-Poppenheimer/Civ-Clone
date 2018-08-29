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


        public bool BalanceResources {
            get {
                return _balanceResources;
            }
        }
        [SerializeField] private bool _balanceResources;


        public float MinFoodPerCell {
            get {
                return _minFoodPerCell;
            }
        }
        [SerializeField] private float _minFoodPerCell;

        public float MinProductionPerCell {
            get {
                return _minProductionPerCell;
            }
        }
        [SerializeField] private float _minProductionPerCell;


        public float MinScorePerCell {
            get {
                return _minScorePerCell;
            }
        }
        [SerializeField] private float _minScorePerCell;

        public float MaxScorePerCell {
            get {
                return _maxScorePerCell;
            }
        }
        [SerializeField] private float _maxScorePerCell;

        #endregion

        #endregion
        
    }

}
