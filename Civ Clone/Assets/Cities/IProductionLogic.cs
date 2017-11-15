using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Cities {

    public interface IProductionLogic {

        #region methods

        List<IProductionProject> GetProjectsAvailableToCity(ICity city);

        int GetProductionProgressPerTurnOnProject(ICity city, IProductionProject project);

        int GetGoldCostToHurryProject(ICity city, IProductionProject project);

        #endregion

    }

}
