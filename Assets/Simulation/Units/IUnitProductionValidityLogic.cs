using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Cities;

namespace Assets.Simulation.Units {

    public interface IUnitProductionValidityLogic {

        #region methods

        bool IsTemplateValidForProductionInCity(IUnitTemplate template, ICity city);

        #endregion

    }

}
