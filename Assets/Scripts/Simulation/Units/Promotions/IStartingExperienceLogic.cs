using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Cities;

namespace Assets.Simulation.Units.Promotions {

    public interface IStartingExperienceLogic {

        #region methods

        int GetStartingExperienceForUnit(IUnit unit, ICity producingCity);

        #endregion

    }

}
