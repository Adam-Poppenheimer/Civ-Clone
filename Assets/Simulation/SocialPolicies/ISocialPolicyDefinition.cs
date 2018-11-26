using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.SocialPolicies {

    public interface ISocialPolicyDefinition {

        #region properties

        string name { get; }

        string Description { get; }

        IEnumerable<ISocialPolicyDefinition> Prerequisites { get; }

        Sprite Icon { get; }

        float TreeNormalizedX { get; }
        float TreeNormalizedY { get; }

        ISocialPolicyBonusesData Bonuses { get; }

        #endregion

    }

}
