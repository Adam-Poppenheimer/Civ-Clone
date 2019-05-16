using System;

using UnityEngine;

using Assets.Simulation.Cities;

namespace Assets.UI.Cities {

    public interface ICityCaptureDisplay {

        #region properties

        ICity TargetedCity { get; set; }

        GameObject gameObject { get; }

        #endregion

        #region methods

        void RazeCity();

        #endregion

    }

}