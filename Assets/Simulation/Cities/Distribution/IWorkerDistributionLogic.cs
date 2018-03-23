using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.WorkerSlots;

namespace Assets.Simulation.Cities.Distribution {

    /// <summary>
    /// Base interface for worker distribution, which is used to determine which are used to assign
    /// open citizens to open slots based on a variety of conditions. Also determines the slots
    /// available to a particular city and how many unemployed people it has.
    /// </summary>
    public interface IWorkerDistributionLogic {

        #region methods

        /// <summary>
        /// Attempts to distribute the given number of citizens into the given
        /// collection of available slots, with the given resource focus and from the 
        /// perspective of the given city.
        /// </summary>
        /// <param name="workerCount">The number of workers to distribute into slots.</param>
        /// <param name="slots">The slots available for distribution</param>
        /// <param name="sourceCity">The city the distribution is being performed for</param>
        /// <param name="preferences">The resource focus the distribution should be trying to maximize</param>
        void DistributeWorkersIntoSlots(int workerCount, IEnumerable<IWorkerSlot> slots,
            ICity sourceCity, ResourceFocusType preferences);

        /// <summary>
        /// Retrieves all slots available to the given city.
        /// </summary>
        /// <param name="city">The city in question.</param>
        /// <returns>All slots that the city has access to and can assign workers to.</returns>
        IEnumerable<IWorkerSlot> GetSlotsAvailableToCity(ICity city);

        #endregion

    }

}
