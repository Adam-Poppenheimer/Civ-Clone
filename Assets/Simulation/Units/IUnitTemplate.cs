using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Units.Abilities;

namespace Assets.Simulation.Units {

    public interface IUnitTemplate {

        #region properties

        string Name { get; }

        int ProductionCost { get; }

        int MaxMovement { get; }

        UnitType Type { get; }

        IEnumerable<UnitAbilityDefinition> Abilities { get; }

        #endregion

    }

}
