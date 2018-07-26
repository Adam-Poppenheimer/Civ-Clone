using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Cities.Buildings {

    public interface IBuildingYieldLogic {

        YieldSummary GetYieldOfBuilding(IBuilding building);

    }

}
