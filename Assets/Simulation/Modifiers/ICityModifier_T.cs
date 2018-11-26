using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Cities;

namespace Assets.Simulation.Modifiers {

    public interface ICityModifier<T> {

        #region methods

        T GetValueForCity(ICity city);

        #endregion

    }

}
