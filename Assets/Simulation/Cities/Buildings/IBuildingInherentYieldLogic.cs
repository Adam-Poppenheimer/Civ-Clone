using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Cities.Buildings {

    public interface IBuildingInherentYieldLogic {

        YieldSummary GetYieldOfBuilding(IBuilding building);

    }

}
