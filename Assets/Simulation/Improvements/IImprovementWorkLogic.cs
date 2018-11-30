using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Units;

namespace Assets.Simulation.Improvements {

    public interface IImprovementWorkLogic {

        #region methods

        float GetWorkOfUnitOnImprovement(IUnit unit, IImprovement improvement);

        #endregion

    }

}
