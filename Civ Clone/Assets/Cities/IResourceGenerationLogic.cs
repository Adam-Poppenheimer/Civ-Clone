using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Cities {

    public interface IResourceGenerationLogic {

        #region methods

        ResourceSummary GetResourcesFromSlots(List<IWorkerSlot> slots);

        ResourceSummary GetResourcesFromBuildings(List<IBuilding> buildings);

        ResourceSummary GetYieldOfSlotForCity(IWorkerSlot slot, ICity city);

        ResourceSummary GetYieldOfUnemployedForCity(ICity city);

        #endregion

    }

}
