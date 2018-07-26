using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.MapResources;

namespace Assets.Simulation.Civilizations {

    public interface IFreeResourcesLogic {

        #region methods

        int GetFreeCopiesOfResourceForCiv(IResourceDefinition resource, ICivilization civ);

        #endregion

    }

}
