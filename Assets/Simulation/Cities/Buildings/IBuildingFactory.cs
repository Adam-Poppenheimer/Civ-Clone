using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Simulation.Cities.Buildings {

    /// <summary>
    /// The base interface for all building factories.
    /// </summary>
    public interface IBuildingFactory : IFactory<IBuildingTemplate, ICity, IBuilding> {

        #region properties

        IEnumerable<IBuilding> AllBuildings { get; }

        #endregion

        #region methods

        /// <summary>
        /// Determines whether a building of the given template could be constructed in
        /// the given city.
        /// </summary>
        /// <param name="template">The template being considered</param>
        /// <param name="city">The city the hypothetical building would be placed into</param>
        /// <returns>Whether the given template is valid in the given city</returns>
        bool CanConstructTemplateInCity(IBuildingTemplate template, ICity city);

        #endregion

    }

}
