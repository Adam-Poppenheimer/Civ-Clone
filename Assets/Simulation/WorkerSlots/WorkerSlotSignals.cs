using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

namespace Assets.Simulation.WorkerSlots {

    public class WorkerSlotSignals {

        #region instance fields and properties

        public ISubject<IWorkerSlot> SlotClicked { get; private set; }

        public ISubject<IWorkerSlot> SlotBecameOccupied  { get; private set; }
        public ISubject<IWorkerSlot> SlotBecameUnoccuped { get; private set; }

        public ISubject<IWorkerSlot> SlotBecameLocked   { get; private set; }
        public ISubject<IWorkerSlot> SlotBecameUnlocked { get; private set; }

        #endregion

        #region constructors

        public WorkerSlotSignals() {
            SlotClicked = new Subject<IWorkerSlot>();

            SlotBecameOccupied  = new Subject<IWorkerSlot>();
            SlotBecameUnoccuped = new Subject<IWorkerSlot>();

            SlotBecameLocked   = new Subject<IWorkerSlot>();
            SlotBecameUnlocked = new Subject<IWorkerSlot>();
        }

        #endregion

    }

}
