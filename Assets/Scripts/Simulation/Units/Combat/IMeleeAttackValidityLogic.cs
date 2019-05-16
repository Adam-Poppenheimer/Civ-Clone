using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Units.Combat {

    public interface IMeleeAttackValidityLogic {

        #region methods

        bool IsMeleeAttackValid(IUnit attacker, IUnit defender);

        #endregion

    }

}
