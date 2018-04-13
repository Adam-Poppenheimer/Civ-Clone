using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Cities;

namespace Assets.Simulation.Civilizations {

    public interface ICapitalCityCanon {

        #region methods

        ICity GetCapitalOfCiv(ICivilization civ);

        bool CanSetCapitalOfCiv(ICivilization civ, ICity newCapital);
        void SetCapitalOfCiv   (ICivilization civ, ICity newCapital);

        #endregion

    }

}
