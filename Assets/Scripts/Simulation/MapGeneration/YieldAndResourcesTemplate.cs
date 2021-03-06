﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.MapGeneration {

    [CreateAssetMenu(menuName = "Civ Clone/Map Generation/Yield and Resources Template")]
    public class YieldAndResourcesTemplate : ScriptableObject, IYieldAndResourcesTemplate {

        #region instance fields and properties

        public float StrategicNodesPerCell {
            get { return _strategicNodesPerCell; }
        }
        [SerializeField] private float _strategicNodesPerCell = 0f;

        public float StrategicCopiesPerCell {
            get { return _strategicCopiesPerCell; }
        }
        [SerializeField] private float _strategicCopiesPerCell = 0f;

        public float MinFoodPerCell {
            get { return _minFoodPerCell; }
        }
        [SerializeField] private float _minFoodPerCell = 0f;

        public float MinProductionPerCell {
            get { return _minProductionPerCell; }
        }
        [SerializeField] private float _minProductionPerCell = 0f;

        public float MinScorePerCell {
            get { return _minScorePerCell; }
        }
        [SerializeField] private float _minScorePerCell = 0f;

        public float MaxScorePerCell {
            get { return _maxScorePerCell; }
        }
        [SerializeField] private float _maxScorePerCell = 0f;

        public float LandWeight {
            get { return _landWeight; }
        }
        [SerializeField, Range(0f, 1f)] private float _landWeight = 1f;

        public float WaterWeight {
            get { return _waterWeight; }
        }
        [SerializeField, Range(0f, 1f)] private float _waterWeight = 1f;

        #endregion

    }

}
