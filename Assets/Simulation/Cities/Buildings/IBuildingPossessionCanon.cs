using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Simulation.Cities.Buildings {

    /// <summary>
    /// The canon for building possession by cities. This class determines what buildings belong
    /// to a city and whether a building can be placed in a city. It also provides methods for
    /// adding and removing buildings from city.
    /// </summary>
    public interface IBuildingPossessionCanon {

        #region methods

        /// <summary>
        /// Returns a collection of all buildings within the argued city.
        /// </summary>
        /// <remarks>
        /// This method should always return a non-null collection, even when the
        /// argued city has no buildings within it. In that case, the collection
        /// should be empty.
        /// </remarks>
        /// <param name="city">The city whose buildings should be retrieved.</param>
        /// <returns>A non-null collection containing all buildings in that city.</returns>
        /// <exception cref="System.ArgumentNullException">Throws whenever the argued city is null.</exception>
        ReadOnlyCollection<IBuilding> GetBuildingsInCity(ICity city);

        /// <summary>
        /// Returns the city a given building belongs to, or null if none exists.
        /// </summary>
        /// <param name="building">The building whose city should be retrieved.</param>
        /// <returns>The city the given building belongs to, or null if one does not exist.</returns>
        ICity GetCityOfBuilding(IBuilding building);

        /// <summary>
        /// Determines whether the given building can be placed in the given city.
        /// </summary>
        /// <param name="building">The building in question.</param>
        /// <param name="city">The city in question.</param>
        /// <returns>Whether that building can be placed in that city.</returns>
        /// <exception cref="ArgumentNullException">Throws when either argument is null.</exception>
        bool CanPlaceBuildingInCity(IBuilding building, ICity city);

        /// <summary>
        /// Places the given building in the given city.
        /// </summary>
        /// <param name="building">The building to place.</param>
        /// <param name="city">The city to place it in.</param>
        /// <exception cref="ArgumentNullException">Throws when either argument is null.</exception>
        /// <exception cref="InvalidOperationException">Throws when <ref>CanPlaceBuildingOfCity</ref> would return false on the given arguments.</exception>
        void PlaceBuildingInCity(IBuilding building, ICity city);

        /// <summary>
        /// Removes the given building from its current city, or does nothing if it has no current city.
        /// </summary>
        /// <param name="building">The building in question.</param>
        void RemoveBuildingFromCurrentCity(IBuilding building);

        #endregion

    }

}
