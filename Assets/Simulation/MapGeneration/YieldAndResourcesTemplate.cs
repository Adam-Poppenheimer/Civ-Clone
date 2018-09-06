using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.MapGeneration {

    [CreateAssetMenu(menuName = "Civ Clone/Yield and Resources Template")]
    public class YieldAndResourcesTemplate : ScriptableObject, IYieldAndResourcesTemplate {

        #region instance fields and properties

        public float StrategicNodesPerCell {
            get { return _strategicNodesPerCell; }
        }
        [SerializeField] private float _strategicNodesPerCell;

        public float StrategicCopiesPerCell {
            get { return _strategicCopiesPerCell; }
        }
        [SerializeField] private float _strategicCopiesPerCell;

        public float MinFoodPerCell {
            get { return _minFoodPerCell; }
        }
        [SerializeField] private float _minFoodPerCell;

        public float MinProductionPerCell {
            get { return _minProductionPerCell; }
        }
        [SerializeField] private float _minProductionPerCell;

        public float MinScorePerCell {
            get { return _minScorePerCell; }
        }
        [SerializeField] private float _minScorePerCell;

        public float MaxScorePerCell {
            get { return _maxScorePerCell; }
        }
        [SerializeField] private float _maxScorePerCell;

        #endregion

    }

}
