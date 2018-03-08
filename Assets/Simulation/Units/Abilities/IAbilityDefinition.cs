using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.Units.Abilities {

    public interface IAbilityDefinition {

        #region properties

        string name { get; }

        Sprite Icon { get; }

        bool RequiresMovement { get; }

        bool ConsumesMovement { get; }

        bool DestroysUnit { get; }

        IEnumerable<AbilityCommandRequest> CommandRequests { get; }

        #endregion

    }

}
