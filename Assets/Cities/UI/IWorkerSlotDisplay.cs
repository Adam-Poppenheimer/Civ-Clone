using System;

using UnityEngine;

namespace Assets.Cities.UI {

    public interface IWorkerSlotDisplay {

        #region properties

        GameObject gameObject { get; }
        Transform transform { get; }

        #endregion

        #region methods

        void DisplayOccupationStatus(bool isOccupied, ICityUIConfig config);

        #endregion

    }

}