using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.SocialPolicies {

    public interface ISocialPolicyCostLogic {

        #region methods

        int GetCostOfNextPolicyForCiv(ICivilization civ);

        #endregion

    }

}
