using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.WorkerSlots {

    public class WorkerSlot : IWorkerSlot {

        #region instance fields and properties

        #region from IWorkerSlot

        public ResourceSummary BaseYield { get; set; }

        public bool IsOccupied {
            get { return _isOccupied; }
            set {
                if(_isOccupied != value) {
                    _isOccupied = value;
                    if(value) {
                        Signals.SlotBecameOccupied.OnNext(this);
                    }else {
                        Signals.SlotBecameUnoccuped.OnNext(this);
                    }
                }
            }
        }
        private bool _isOccupied;

        public bool IsLocked {
            get { return _isLocked; }
            set {
                if(_isLocked != value) {
                    _isLocked = value;
                    if(value) {
                        Signals.SlotBecameLocked.OnNext(this);
                    }else {
                        Signals.SlotBecameUnlocked.OnNext(this);
                    }
                }
            }
        }
        private bool _isLocked;

        #endregion

        private WorkerSlotSignals Signals;

        #endregion

        #region constructors

        [Inject]
        public WorkerSlot(WorkerSlotSignals signals) {
            Signals = signals;
        }

        #endregion

        #region instance methods

        #region from Object

        public override string ToString() {
            return string.Format("Slot: {0}", BaseYield);
        }

        #endregion

        #endregion

    }

}
