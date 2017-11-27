using System;

using UnityEngine;
using UnityEngine.UI;

namespace Assets.UI.Cities {

    public interface IWorkerSlotDisplay {

        #region properties

        GameObject gameObject { get; }
        Transform transform { get; }

        Image SlotImage { get; set; }

        #endregion

        #region methods

        void DisplayOccupationStatus(bool isOccupied, ICityUIConfig config);

        #endregion

    }

}