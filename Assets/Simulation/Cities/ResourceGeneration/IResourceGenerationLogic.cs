﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.WorkerSlots;

namespace Assets.Simulation.Cities.ResourceGeneration {

    /// <summary>
    /// Determines the total resource yield generated by a city, including changes
    /// due to modifiers. Performs a similar task for individual slots and for
    /// unemployed citizens.
    /// </summary>
    public interface IResourceGenerationLogic {

        #region methods

        /// <summary>
        /// Determines the total amount of resources the given city generates per turn,
        /// including all slots, all static yield, and all modifiers.
        /// </summary>
        /// <param name="city">The city in question.</param>
        /// <returns>The total yield of that city per turn.</returns>
        /// <exception cref="ArgumentNullException">Throws when city is null</exception>
        ResourceSummary GetTotalYieldForCity(ICity city);

        /// <summary>
        /// Determines the total yield per turn of the given slot for the given city, taking into
        /// account base yield and all modifiers.
        /// </summary>
        /// <param name="slot">The slot in question.</param>
        /// <param name="city">The city in question.</param>
        /// <returns>The total yield of that slot for that city.</returns>
        /// <exception cref="ArgumentNullException">Throws when slot or city is null</exception>
        ResourceSummary GetYieldOfSlotForCity(IWorkerSlot slot, ICity city);

        /// <summary>
        /// Determines the yield per turn of an unemployed citizen in the given city,
        /// taking into account all modifiers.
        /// </summary>
        /// <param name="city">The city in question.</param>
        /// <returns>The yield per turn of a single unemployed person.</returns>
        /// <exception cref="ArgumentNullException">Throws when city is null</exception>
        ResourceSummary GetYieldOfUnemployedForCity(ICity city);

        #endregion

    }

}
