using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Cities {

    public interface ICityHappinessLogic {

        #region methods

        int GetLocalHappinessOfCity (ICity city);
        int GetGlobalHappinessOfCity(ICity city);
        int GetTotalHappinessofCity (ICity city);

        int GetUnhappinessOfCity    (ICity city);
        int GetNetHappinessOfCity   (ICity city);

        #endregion

    }

}
