using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units.Combat {

    public interface ICombatInfoLogic {

        #region methods

        CombatInfo GetMeleeAttackInfo (IUnit attacker, IUnit defender, IHexCell location);
        CombatInfo GetRangedAttackInfo(IUnit attacker, IUnit defender, IHexCell location);

        #endregion

    }

}
