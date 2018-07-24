using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    [Serializable]
    public struct MapSection {

        #region instance fields and properties

        public float XMin {
            get { return _xMin; }
        }
        [SerializeField, Range(0f, 1f)] private float _xMin;

        public float XMax {
            get { return _xMax; }
        }
        [SerializeField, Range(0f, 1f)] private float _xMax;

        public float ZMin {
            get { return _zMin; }
        }
        [SerializeField, Range(0f, 1f)] private float _zMin;

        public float ZMax {
            get { return _zMax; }
        }
        [SerializeField, Range(0f, 1f)] private float _zMax;

        #endregion

    }

}
