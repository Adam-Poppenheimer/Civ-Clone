using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Cities {

    public interface IResourceGenerationLogic {

        #region methods

        ResourceSummary GetTotalYieldForCity(ICity city);

        ResourceSummary GetYieldOfSlotForCity(IWorkerSlot slot, ICity city);

        ResourceSummary GetYieldOfUnemployedForCity(ICity city);

        #endregion

    }

}
