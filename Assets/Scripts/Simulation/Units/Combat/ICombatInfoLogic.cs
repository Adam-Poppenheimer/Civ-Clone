using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units.Combat {

    public interface ICombatInfoLogic {

        #region methods

        CombatInfo GetAttackInfo(IUnit attacker, IUnit defender, IHexCell location, CombatType combatType);

        #endregion

    }

}
