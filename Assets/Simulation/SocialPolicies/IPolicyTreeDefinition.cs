using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.SocialPolicies {

    public interface IPolicyTreeDefinition {

        #region properties

        string name { get; }

        string Description { get; }

        IEnumerable<ISocialPolicyDefinition> Policies { get; }

        Sprite Icon { get; }

        int Row    { get; }
        int Column { get; }

        #endregion

    }

}
