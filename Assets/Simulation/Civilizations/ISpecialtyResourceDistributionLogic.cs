using System;
using System.Collections.Generic;

using Assets.Simulation.Cities;

namespace Assets.Simulation.Civilizations {

    public interface ISpecialtyResourceDistributionLogic {

        #region methods

        void DistributeResourcesOfCiv(ICivilization civ);

        #endregion

    }

}