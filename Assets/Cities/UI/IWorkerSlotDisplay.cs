using System;

using UnityEngine;

namespace Assets.Cities.UI {

    public interface IWorkerSlotDisplay {

        #region properties

        bool IsOccupied { get; set; }

        GameObject gameObject { get; }
        Transform transform { get; }

        #endregion

        #region methods

        void Refresh();

        #endregion

    }

}