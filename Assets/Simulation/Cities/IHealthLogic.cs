using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Cities {

    public interface IHealthLogic {

        #region methods

        int GetHealthOfCity(ICity city);

        #endregion

    }

}
