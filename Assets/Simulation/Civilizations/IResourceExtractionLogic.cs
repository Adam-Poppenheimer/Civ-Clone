using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.SpecialtyResources;

namespace Assets.Simulation.Civilizations {

    public interface IResourceExtractionLogic {

        #region methods

        int GetExtractedCopiesOfResourceForCiv(ISpecialtyResourceDefinition resource, ICivilization civ);

        #endregion

    }

}
