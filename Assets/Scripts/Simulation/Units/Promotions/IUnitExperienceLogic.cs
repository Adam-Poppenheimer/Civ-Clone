using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Units.Promotions {

    public interface IUnitExperienceLogic {

        #region methods

        int GetExperienceForNextLevelOnUnit(IUnit unit);

        #endregion

    }

}
