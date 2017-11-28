using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Cities.Buildings;

namespace Assets.Simulation.Cities.Production {

    public interface IProductionProject {

        #region properties

        string Name { get; }

        int Progress { get; set; }
        int ProductionToComplete { get; }

        IBuildingTemplate BuildingTemplate { get; }

        #endregion

        #region methods

        void Execute(ICity targetCity);

        #endregion

    }

}
