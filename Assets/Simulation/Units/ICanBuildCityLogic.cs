using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Units {

    public interface ICanBuildCityLogic {

        #region methods

        bool CanUnitBuildCity(IUnit unit);

        #endregion

    }

}
