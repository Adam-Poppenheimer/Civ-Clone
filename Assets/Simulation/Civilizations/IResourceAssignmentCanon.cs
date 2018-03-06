﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Cities;
using Assets.Simulation.SpecialtyResources;

namespace Assets.Simulation.Civilizations {

    public interface IResourceAssignmentCanon {

        #region instance methods

        int GetFreeCopiesOfResourceForCiv(ISpecialtyResourceDefinition resource, ICivilization civ);
        
        bool CanReserveCopyOfResourceForCiv(ISpecialtyResourceDefinition resource, ICivilization civ);
        void ReserveCopyOfResourceForCiv   (ISpecialtyResourceDefinition resource, ICivilization civ);

        bool CanUnreserveCopyOfResourceForCiv(ISpecialtyResourceDefinition resource, ICivilization civ);
        void UnreserveCopyOfResourceForCiv   (ISpecialtyResourceDefinition resource, ICivilization civ);

        IEnumerable<ISpecialtyResourceDefinition> GetAllFreeResourcesForCiv(ICivilization civ);

        #endregion

    }

}
