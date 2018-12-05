using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Cities {

    public interface ICityModifiers {

        #region properties

        ICityModifier<YieldSummary> BonusYield { get; }

        ICityModifier<float> Growth           { get; }
        ICityModifier<float> BorderExpansionCost  { get; }

        ICityModifier<float> PerPopulationHappiness   { get; }
        ICityModifier<float> PerPopulationUnhappiness { get; }

        ICityModifier<float> GarrisionedRangedCombatStrength { get; }

        #endregion

    }

}
