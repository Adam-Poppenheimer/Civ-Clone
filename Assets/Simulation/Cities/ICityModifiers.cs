using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Modifiers;

namespace Assets.Simulation.Cities {

    public interface ICityModifiers {

        #region properties

        ICityModifier<float> Growth           { get; }
        ICityModifier<float> BorderExpansion  { get; }

        ICityModifier<float> PerPopulationHappiness   { get; }
        ICityModifier<float> PerPopulationUnhappiness { get; }

        ICityModifier<float> GarrisionedRangedCombatStrength { get; }

        #endregion

    }

}
