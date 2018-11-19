using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units.Combat {

    public interface ICombatModifier {

        #region properties

        float Modifier { get; }

        #endregion

        #region methods

        bool DoesModifierApply(IUnit candidate, IUnit opponent, IHexCell location, CombatType combatType);

        #endregion

    }

}
