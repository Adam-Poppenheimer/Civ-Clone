using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Cities;

namespace Assets.Simulation.Units {

    public interface IUnitGarrisonLogic {

        #region methods

        bool IsUnitGarrisoned(IUnit unit);
        bool IsCityGarrisoned(ICity city);

        #endregion

    }

}
