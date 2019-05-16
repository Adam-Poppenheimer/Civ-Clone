using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Cities.Buildings {

    /// <summary>
    /// Determines what templates are valid for construction in various cities.
    /// </summary>
    public interface IBuildingProductionValidityLogic {

        #region methods

        /// <summary>
        /// Retrieves all templates that are valid for construction in the given city.
        /// </summary>
        /// <param name="city">The city to get valid templates for.</param>
        /// <returns>A collection of all valid templates for the given city</returns>
        /// <exception cref="ArgumentNullException">Throws whenever city is null</exception>
        IEnumerable<IBuildingTemplate> GetTemplatesValidForCity(ICity city);

        /// <summary>
        /// Determines whether the given template is valid for construction in the given city.
        /// </summary>
        /// <param name="template">The template in question</param>
        /// <param name="city">The city in question</param>
        /// <returns>Whether that template can be constructed as a building in that city</returns>
        bool IsTemplateValidForCity(IBuildingTemplate template, ICity city);

        #endregion

    }

}
