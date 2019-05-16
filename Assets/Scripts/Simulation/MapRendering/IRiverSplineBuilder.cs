using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Util;

namespace Assets.Simulation.MapRendering {

    public interface IRiverSplineBuilder {

        #region properties

        ReadOnlyCollection<RiverSpline> LastBuiltRiverSplines { get; }

        #endregion

        #region methods

        void RefreshRiverSplines();

        #endregion

    }

}
