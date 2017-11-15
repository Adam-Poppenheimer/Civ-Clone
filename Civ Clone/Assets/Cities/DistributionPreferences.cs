using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Cities {

    public class DistributionPreferences {

        #region instance fields and properties

        public bool ShouldFocusResource { get; set; }

        public ResourceType FocusedResource { get; set; }

        #endregion

        #region constructors

        public DistributionPreferences(bool shouldFocusResource, ResourceType focusedResource) {
            ShouldFocusResource = shouldFocusResource;
            FocusedResource = focusedResource;
        }

        #endregion

    }

}
