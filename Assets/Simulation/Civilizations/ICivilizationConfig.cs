using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Civilizations {

    /// <summary>
    /// Defines a variety of fields that configure the behavior of Civilizations
    /// and other similar classes.
    /// </summary>
    public interface ICivilizationConfig {

        #region properties

        int BaseHappiness { get; }

        int HappinessPerLuxury { get; }

        float YieldLossPerUnhappiness { get; }

        float ModifierLossPerUnhappiness { get; }

        int BasePolicyCost { get; }

        float PolicyCostPerPolicyCoefficient { get; }
        float PolicyCostPerPolicyExponent    { get; }

        float PolicyCostPerCityCoefficient { get; }

        CivilizationDefeatMode DefeatMode { get; }

        int MaintenanceFreeUnits { get; }

        float CivilianGreatPersonStartingCost { get; }
        float MilitaryGreatPersonStartingCost { get; }

        float GreatPersonPredecessorCostMultiplier { get; }

        #endregion

    }

}
