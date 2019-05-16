using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Cities.Production {

    /// <summary>
    /// Determines the progress per turn of a given project on a given city,
    /// as well as the gold cost to hurry a particular project.
    /// </summary>
    public interface IProductionLogic {

        #region methods

        /// <summary>
        /// Determines the production progress the given city would have if it was
        /// working on the given project.
        /// </summary>
        /// <param name="city">The city in question.</param>
        /// <param name="project">The project in question.</param>
        /// <returns>The expected production progress.</returns>
        /// <exception cref="ArgumentNullException">Throws whenever either argument is null</exception>
        int GetProductionProgressPerTurnOnProject(ICity city, IProductionProject project);

        /// <summary>
        /// Determines the cost in gold of hurrying the given project if in the given
        /// city.
        /// </summary>
        /// <param name="city">The city in question.</param>
        /// <param name="project">The project in question.</param>
        /// <returns>The expected hurry cost.</returns>
        /// <exception cref="ArgumentNullException">Throws whenever either argument is null</exception> 
        int GetGoldCostToHurryProject(ICity city, IProductionProject project);

        #endregion

    }

}
