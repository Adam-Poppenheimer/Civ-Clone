using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.Cities;

namespace Assets.Simulation.Civilizations {

    public interface ICivilizationTemplate {

        #region properties

        string Name { get; }

        Color Color { get; }

        #endregion

        #region methods

        string GetNextName(IEnumerable<ICity> existingCities);

        #endregion

    }

}
