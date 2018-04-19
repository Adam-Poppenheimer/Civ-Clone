using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.SpecialtyResources {

    public interface IExtractionValidityLogic {

        #region methods

        bool IsNodeValidForExtraction(IResourceNode node);

        #endregion

    }

}
