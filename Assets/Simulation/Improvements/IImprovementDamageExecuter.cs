using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Units;

namespace Assets.Simulation.Improvements {

    public interface IImprovementDamageExecuter {

        #region methods

        void PerformDamageOnUnitFromImprovements(IUnit unit);

        #endregion

    }

}
