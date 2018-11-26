using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Cities.ResourceGeneration {

    public interface ICityCenterYieldLogic {

        #region methods

        YieldSummary GetYieldOfCityCenter(ICity city);

        #endregion 

    }

}
