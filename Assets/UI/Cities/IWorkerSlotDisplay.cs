using System;

using Assets.Simulation;

namespace Assets.UI.Cities {

    public interface IWorkerSlotDisplay {

        #region properties

        IWorkerSlot SlotToDisplay { get; set; }

        #endregion

        #region methods

        void Refresh();

        #endregion

    }

}