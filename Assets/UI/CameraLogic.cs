using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.UI {

    public class CameraLogic : ITickable {

        #region instance fields and properties

        private Camera CameraToControl;

        private ICameraConfig Config;

        #endregion

        #region constructors

        [Inject]
        public CameraLogic([Inject(Id = "Main Camera")] Camera cameraToControl, ICameraConfig config) {
            CameraToControl = cameraToControl;
            Config = config;
        }

        #endregion

        #region instance methods

        #region from ITickable

        public void Tick() {
            CameraToControl.transform.position += new Vector3(
                Input.GetAxis("Horizontal") * Config.PanningSpeed,
                Input.GetAxis("Vertical") * Config.PanningSpeed
            );

            CameraToControl.fieldOfView = Mathf.Clamp(
                CameraToControl.fieldOfView + -Input.GetAxis("Mouse ScrollWheel") * Config.ZoomingSpeed,
                Config.MinFOV,
                Config.MaxFOV
            );
        }

        #endregion

        #endregion

    }

}
