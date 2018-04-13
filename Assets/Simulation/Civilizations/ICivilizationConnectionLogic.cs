using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Civilizations {

    public interface ICivilizationConnectionLogic {

        #region methods

        bool AreCivilizationsConnected(ICivilization civOne, ICivilization civTwo);

        #endregion

    }

}
