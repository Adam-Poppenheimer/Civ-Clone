using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units.Combat {

    public interface ICombatEstimator {

        #region methods

        UnitCombatResults EstimateMeleeAttackResults (IUnit attacker, IUnit defender, IHexCell location);
        UnitCombatResults EstimateRangedAttackResults(IUnit attacker, IUnit defender, IHexCell location);

        #endregion

    }

}
