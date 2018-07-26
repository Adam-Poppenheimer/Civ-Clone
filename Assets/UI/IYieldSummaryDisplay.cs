using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation;

namespace Assets.UI {

    public interface IYieldSummaryDisplay {

        #region methods

        void DisplaySummary(YieldSummary summary);

        #endregion

    }

}
