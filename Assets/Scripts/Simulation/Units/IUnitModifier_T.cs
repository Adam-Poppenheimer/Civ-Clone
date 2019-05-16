using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Units {

    public interface IUnitModifier<T> {

        #region methods

        T GetValueForUnit(IUnit unit);

        #endregion

    }

}
