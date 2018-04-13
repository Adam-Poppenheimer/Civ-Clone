using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities;

namespace Assets.Simulation.Civilizations {

    public class CapitalCityCanon : ICapitalCityCanon {

        #region instance fields and properties

        private Dictionary<ICivilization, ICity> CapitalDictionary = 
            new Dictionary<ICivilization, ICity>();

        private CivilizationSignals CivSignals;

        #endregion

        #region constructors

        [Inject]
        public CapitalCityCanon(CivilizationSignals civSignals) {
            CivSignals = civSignals;
        }

        #endregion

        #region instance methods

        #region from ICapitalCityCanon

        public ICity GetCapitalOfCiv(ICivilization civ) {
            ICity retval;
            CapitalDictionary.TryGetValue(civ, out retval);
            return retval;
        }

        public bool CanSetCapitalOfCiv(ICivilization civ, ICity city) {
            return GetCapitalOfCiv(civ) != city;
        }

        public void SetCapitalOfCiv(ICivilization civ, ICity city) {
            if(!CanSetCapitalOfCiv(civ, city)) {
                throw new InvalidOperationException("CanSetCapitalOfCiv must return true on the given arguments");
            }

            CapitalDictionary[civ] = city;
        }

        #endregion

        #endregion
        
    }

}
