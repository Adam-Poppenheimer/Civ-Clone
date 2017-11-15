using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Cities {

    public class ResourceGenerationLogic : IResourceGenerationLogic {

        #region instance methods

        #region from IResourceGenerationLogic

        public ResourceSummary GetResourcesFromBuildings(List<IBuilding> buildings) {
            throw new NotImplementedException();
        }

        public ResourceSummary GetResourcesFromSlots(List<IWorkerSlot> slots) {
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
