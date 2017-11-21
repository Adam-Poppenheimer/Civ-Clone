using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Cities.Buildings;

namespace Assets.Cities.Production {

    public interface IProductionProjectFactory {

        #region methods

        BuildingProductionProject ConstructBuildingProject(IBuildingTemplate template);

        #endregion

    }

}
