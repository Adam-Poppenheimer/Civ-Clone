using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units.Combat {

    public interface ICombatModifierLogic {

        #region methods

        float GetMeleeDefensiveModifierAtLocation(IUnit attacker, IUnit defender, IHexCell location);
        float GetMeleeOffensiveModifierAtLocation(IUnit attacker, IUnit defender, IHexCell location);

        float GetRangedDefensiveModifierAtLocation(IUnit attacker, IUnit defender, IHexCell location);
        float GetRangedOffensiveModifierAtLocation(IUnit attacker, IUnit defender, IHexCell location);

        #endregion

    }

}
