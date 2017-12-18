using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Cities.ResourceGeneration {

    public interface IIncomeModifierLogic {

        #region methods

        ResourceSummary GetRealBaseYieldForSlot(IWorkerSlot slot);

        ResourceSummary GetYieldMultipliersForSlot(IWorkerSlot slot);

        ResourceSummary GetYieldMultipliersForCity(ICity city);

        ResourceSummary GetYieldMultipliersForCivilization(ICivilization civilization);

        #endregion

    }

}
