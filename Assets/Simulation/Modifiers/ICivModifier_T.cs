using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Modifiers {

    public interface ICivModifier<T> {

        #region methods

        T GetValueForCiv(ICivilization civ);

        #endregion

    }

}
