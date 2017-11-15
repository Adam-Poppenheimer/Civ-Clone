using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Cities {

    public class ResourceGenerationLogic : IResourceGenerationLogic {        

        #region instance methods

        #region from IResourceGenerationLogic

        public ResourceSummary GetTotalYieldForCity(ICity city) {
            throw new NotImplementedException();
        }

        public ResourceSummary GetYieldOfSlotForCity(IWorkerSlot slot, ICity city) {
            throw new NotImplementedException();
        }

        public ResourceSummary GetYieldOfUnemployedForCity(ICity city) {
            throw new NotImplementedException();
        }

        #endregion

        #endregion

    }

}
