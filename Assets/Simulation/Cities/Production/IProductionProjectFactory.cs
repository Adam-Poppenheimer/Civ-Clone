using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Cities.Buildings;

namespace Assets.Simulation.Cities.Production {

    public interface IProductionProjectFactory {

        #region methods

        IProductionProject ConstructBuildingProject(IBuildingTemplate template);

        #endregion

    }

}
