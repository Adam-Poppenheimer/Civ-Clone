using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Cities {

    class ProductionLogic : IProductionLogic {

        #region instance methods

        #region from IProductionLogic

        public int GetGoldCostToHurryProject(ICity city, IProductionProject project) {
            throw new NotImplementedException();
        }

        public int GetProductionProgressPerTurnOnProject(ICity city, IProductionProject project) {
            throw new NotImplementedException();
        }

        public List<IProductionProject> GetProjectsAvailableToCity(ICity city) {
            throw new NotImplementedException();
        }

        #endregion

        #endregion

    }

}
