using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Cities {

    public interface ICapitalConnectionLogic {

        #region methods

        bool IsCityConnectedToCapital(ICity city);

        #endregion

    }

}
