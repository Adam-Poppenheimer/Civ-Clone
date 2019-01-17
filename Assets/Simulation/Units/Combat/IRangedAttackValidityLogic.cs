using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Units.Combat {

    public interface IRangedAttackValidityLogic {

        #region methods

        bool IsRangedAttackValid(IUnit attacker, IUnit defender);

        #endregion

    }

}
