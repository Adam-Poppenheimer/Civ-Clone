using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Units.Combat {

    public interface ICommonAttackValidityLogic {

        #region methods

        bool DoesAttackMeetCommonConditions(IUnit attacker, IUnit defender);

        #endregion

    }

}
