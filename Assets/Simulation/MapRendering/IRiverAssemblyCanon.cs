using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.MapRendering {

    public interface IRiverAssemblyCanon {

        #region properties

        ReadOnlyCollection<ReadOnlyCollection<RiverSection>> Rivers { get; }

        IEnumerable<RiverSection> UnassignedSections { get; }

        #endregion

        #region methods

        void RefreshRivers();

        #endregion

    }

}
