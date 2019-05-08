using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UniRx;

namespace Assets.Simulation.Units.Combat {

    public interface ICombatCalculator {

        #region methods

        UniRx.Tuple<int, int> CalculateCombat(IUnit attacker, IUnit defender, CombatInfo combatInfo);

        #endregion

    }

}
