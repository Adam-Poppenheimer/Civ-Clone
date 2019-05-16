using System;
using System.Collections.Generic;

namespace Assets.Simulation.MapRendering {

    public interface IRiverBuilderUtilities {

        #region methods

        RiverSection GetNextActiveSectionForRiver(
            RiverSection activeSection, RiverSection lastSection, HashSet<RiverSection> unassignedSections
        );

        void SetCurveStatusOfSection(RiverSection section, List<RiverSection> river);

        #endregion

    }

}