using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Units.Combat {

    public interface ICombatEstimator {

        #region methods

        UnitCombatResults EstimateMeleeAttackResults (IUnit attacker, IUnit defender);
        UnitCombatResults EstimateRangedAttackResults(IUnit attacker, IUnit defender);

        #endregion

    }

}
