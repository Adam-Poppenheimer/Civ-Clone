﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;
using Assets.Simulation.WorkerSlots;

namespace Assets.Simulation.Cities.ResourceGeneration {

    /// <summary>
    /// Class that calculates the income modifiers and base yield for a variety of 
    /// entities. These are used to determine the real yield of a particular slot
    /// by a particular city belonging to a particular civilization.
    /// </summary>
    public interface IIncomeModifierLogic {

        #region methods

        /// <summary>
        /// Retrieves the yield modifiers for all income generated by the given city, disregarding
        /// other modifiers from that city's civilization.
        /// </summary>
        /// <param name="city">The city in question</param>
        /// <returns>
        /// A ResourceSummary object whose values represent percentage changes from the slot's base
        /// yield. ResourceSummary.Empty represents no change, positive values increase yield,
        /// and negative values decrease yield.
        /// </returns>
        YieldSummary GetYieldMultipliersForCity(ICity city);

        /// <summary>
        /// Retrieves the yield modifiers for all income generated by cities belonging to the given
        /// civilization.
        /// </summary>
        /// <param name="civilization">The civilization in question.</param>
        /// <returns>
        /// A ResourceSummary object whose values represent percentage changes from the slot's base
        /// yield. ResourceSummary.Empty represents no change, positive values increase yield,
        /// and negative values decrease yield.
        /// </returns>
        YieldSummary GetYieldMultipliersForCivilization(ICivilization civilization);

        #endregion

    }

}