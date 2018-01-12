using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Units;

namespace Assets.Simulation.Cities.Production {

    /// <summary>
    /// The base interface for all production project factories.
    /// </summary>
    public interface IProductionProjectFactory {

        #region methods

        /// <summary>
        /// Constructs a production project that, when executed, creates a building
        /// of the given template.
        /// </summary>
        /// <param name="template">The template in question.</param>
        /// <returns>A production project that can create a building of that template.</returns>
        IProductionProject ConstructBuildingProject(IBuildingTemplate template);

        /// <summary>
        /// Constructs a production project that, when executed, creates a unit
        /// of the given template.
        /// </summary>
        /// <param name="template">The template in question.</param>
        /// <returns>A production project that can create a unit of that template.</returns>
        IProductionProject ConstructUnitProject(IUnitTemplate template);

        #endregion

    }

}
