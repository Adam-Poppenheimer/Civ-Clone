using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Civilizations {

    public interface ICivilizationFactory {

        #region properties

        IEnumerable<ICivilization> AllCivilizations { get; }

        #endregion

    }

}
