using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Units.Abilities {

    public interface IUnitAbilityDefinition {

        #region properties

        string name { get; }

        IEnumerable<AbilityCommandRequest> CommandRequests { get; }

        #endregion

    }

}
