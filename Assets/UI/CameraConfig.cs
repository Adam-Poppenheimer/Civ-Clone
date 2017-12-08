using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.UI {

    [CreateAssetMenu(menuName = "Civ Clone/Camera Config")]
    public class CameraConfig : ScriptableObject, ICameraConfig {

        #region instance fields and properties

        #region from ICameraConfig

        public float PanningSpeed {
            get { return _panningSpeed; }
        }
        [SerializeField] private float _panningSpeed;

        public float ZoomingSpeed {
            get { return _zoomingSpeed; }
        }
        [SerializeField] private float _zoomingSpeed;

        public float MinFOV {
            get { return _minFOV; }
        }
        [SerializeField] private float _minFOV;

        public float MaxFOV {
            get { return _maxFOV; }
        }
        [SerializeField] private float _maxFOV;

        #endregion

        #endregion
        
    }

}
