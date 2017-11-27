using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation;

namespace Assets.UI {

    public interface IResourceSummaryDisplay {

        #region methods

        void DisplaySummary(ResourceSummary summary);

        #endregion

    }

}
