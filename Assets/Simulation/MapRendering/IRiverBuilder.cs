using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.MapRendering {

    public interface IRiverBuilder {

        #region methods

        List<RiverSection> BuildRiverFromSection(RiverSection section, HashSet<RiverSection> unassignedRiverSections);

        #endregion

    }

}
