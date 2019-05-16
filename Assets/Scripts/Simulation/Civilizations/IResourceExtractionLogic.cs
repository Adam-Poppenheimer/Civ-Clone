using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.MapResources;

namespace Assets.Simulation.Civilizations {

    public interface IResourceExtractionLogic {

        #region methods

        int GetExtractedCopiesOfResourceForCiv(IResourceDefinition resource, ICivilization civ);

        #endregion

    }

}
