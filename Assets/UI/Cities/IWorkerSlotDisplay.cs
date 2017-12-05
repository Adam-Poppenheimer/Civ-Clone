using System;

using UnityEngine;

using Zenject;

using Assets.Simulation;

namespace Assets.UI.Cities {

    public interface IWorkerSlotDisplay {

        #region properties

        GameObject gameObject { get; }

        IWorkerSlot SlotToDisplay { get; set; }

        #endregion

        #region methods

        void Refresh();

        #endregion

    }

    public class WorkerSlotDisplayFactory : Factory<IWorkerSlotDisplay> {  }

}