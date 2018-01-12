using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Cities.Buildings;

namespace Assets.Simulation.Cities.Production {

    /// <summary>
    /// The base interface for all production projects, which are undertaken
    /// by cities to add new entities to the world, like buildings or units.
    /// </summary>
    public interface IProductionProject {

        #region properties

        /// <summary>
        /// The name of the project.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// How much progress has been made on the project so far.
        /// </summary>
        int Progress { get; set; }

        /// <summary>
        /// How much progress is required to complete the project.
        /// </summary>
        int ProductionToComplete { get; }

        #endregion

        #region methods

        /// <summary>
        /// Executes the project on the argued city, creating whatever entity
        /// the project was meant to build upon its completion.
        /// </summary>
        /// <param name="targetCity">The city to execute the project from.</param>
        void Execute(ICity targetCity);

        #endregion

    }

}
