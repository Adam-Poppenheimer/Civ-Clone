using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Cities.Territory {

    /// <summary>
    /// Defines and controls the ownership of cells by cities, which determines
    /// which cells are within which cities' borders.
    /// </summary>
    public interface ICellPossessionCanon {

        #region methods

        /// <summary>
        /// Retrieves all cells belonging to the given city.
        /// </summary>
        /// <param name="city">The city in question.</param>
        /// <returns>A non-null collection containing all cells belonging to that city.</returns>
        /// <exception cref="ArgumentNullException">Throws when city is null.</exception>
        IEnumerable<IHexCell> GetTilesOfCity(ICity city);

        /// <summary>
        /// Retrives the city that owns the given cell, if one exists.
        /// </summary>
        /// <param name="cell">The cell in question.</param>
        /// <returns>The city that owns the cell, or null if none exists.</returns>
        /// <exception cref="ArgumentNullException">Throws when cell is null.</exception>
        ICity GetCityOfTile(IHexCell cell);

        /// <summary>
        /// Determines whether the given cell can have its owner changed to the given city.
        /// </summary>
        /// <param name="cell">The cell in question.</param>
        /// <param name="newOwner">The city in question.</param>
        /// <returns>Whether the ownership transfer is valid.</returns>
        /// <exception cref="ArgumentNullException">Throws when cell is null.</exception>
        bool CanChangeOwnerOfTile(IHexCell cell, ICity newOwner);

        /// <summary>
        /// Changes the owner of the given cell to the given city, or removes all ownership
        /// if newOwner is null.
        /// </summary>
        /// <param name="cell">The cell in question.</param>
        /// <param name="newOwner">The city in question, or null if no ownership is desired.</param>
        /// <exception cref="ArgumentNullException">Throws when cell is null.</exception>
        /// <exception cref="InvalidOperationException">Throws when CanChangeOwnerOfTile would return false on the given arguments</exception>
        void ChangeOwnerOfTile(IHexCell cell, ICity newOwner);

        #endregion

    }

}
