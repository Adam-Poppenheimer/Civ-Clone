using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Civilizations {

    public interface ICivilizationYieldLogic {

        #region methods

        YieldSummary GetYieldOfCivilization(ICivilization civilization);

        #endregion

    }

}
