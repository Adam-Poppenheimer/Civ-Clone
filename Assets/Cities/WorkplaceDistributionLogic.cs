using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.GameMap;

namespace Assets.Cities {

    public class WorkplaceDistributionLogic : IWorkerDistributionLogic {

        #region instance fields and properties

        private IPopulationGrowthLogic GrowthLogic;

        private IResourceGenerationLogic GenerationLogic;

        #endregion

        #region constructors

        [Inject]
        public WorkplaceDistributionLogic(IPopulationGrowthLogic growthLogic, IResourceGenerationLogic generationLogic) {
            GrowthLogic = growthLogic;
            GenerationLogic = generationLogic;
        }

        #endregion

        #region instance methods

        #region from IWorkerDistributionLogic

        public void DistributeWorkersIntoSlots(int workerCount, List<IWorkerSlot> slots, DistributionPreferences preferences) {
            throw new NotImplementedException();
        }

        #endregion

        #endregion
        
    }

}
