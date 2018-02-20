using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.SpecialtyResources {

    public interface ISpecialtyResourcePossessionLogic {

        #region methods

        int GetCopiesOfResourceBelongingToCiv(ISpecialtyResourceDefinition resource, ICivilization civ);

        Dictionary<ISpecialtyResourceDefinition, int> GetFullResourceSummaryForCiv(ICivilization civ);

        #endregion

    }

}
