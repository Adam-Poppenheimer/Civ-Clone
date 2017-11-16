using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Cities {

    public class BorderExpansionConfig : ScriptableObject, IBorderExpansionConfig {

        #region instance fields and properties

        #region from IBorderExpansionConfig

        public int MaxRange {
            get { return _maxRange; }
        }
        [SerializeField] private int _maxRange;

        public int TileCostBase {
            get { return _tileCostBase; }
        }
        [SerializeField] private int _tileCostBase;

        public int PreviousTileCountCoefficient {
            get { return _previousTileCountCoefficient; }
        }
        [SerializeField] private int _previousTileCountCoefficient;

        public float PreviousTileCountExponent {
            get { return _previousTileCountExponent; }
        }
        [SerializeField] private int _previousTileCountExponent;

        #endregion

        #endregion

    }

}
