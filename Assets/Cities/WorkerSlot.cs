using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Cities {

    public class WorkerSlot : IWorkerSlot {

        #region instance fields and properties

        #region from IWorkerSlot

        public ResourceSummary BaseYield {
            get {
                throw new NotImplementedException();
            }
        }

        public bool IsLocked {
            get {
                throw new NotImplementedException();
            }

            set {
                throw new NotImplementedException();
            }
        }

        public bool IsOccupied {
            get {
                throw new NotImplementedException();
            }

            set {
                throw new NotImplementedException();
            }
        }

        #endregion

        #endregion

        #region instance methods

        #region from IWorkerSlot



        #endregion

        #endregion
        
    }

}
