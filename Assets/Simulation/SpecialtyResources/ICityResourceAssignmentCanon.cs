using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.SpecialtyResources {

    interface ICityResourceAssignmentCanon {

        #region instance methods

        bool HasResourceBeenAssignedToCity(ISpecialtyResourceDefinition resource, ICity city);

        IEnumerable<ISpecialtyResourceDefinition> GetAllResourcesAssignedToCity(ICity city);

        int GetFreeCopiesOfResourceForCiv(ISpecialtyResourceDefinition resource, ICivilization civ);

        bool CanUnassignResourceFromCity(ISpecialtyResourceDefinition resource, ICity city);
        void UnassignResourceFromCity   (ISpecialtyResourceDefinition resource, ICity city);

        bool CanAssignResourceToCity(ISpecialtyResourceDefinition resource, ICity city);
        void AssignResourceToCity   (ISpecialtyResourceDefinition resource, ICity city);

        void UnassignAllResourcesFromCity(ICity city);

        #endregion

    }

}
