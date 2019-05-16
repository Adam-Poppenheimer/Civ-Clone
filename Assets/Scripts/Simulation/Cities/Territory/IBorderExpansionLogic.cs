using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Cities.Territory {

    /// <summary>
    /// Defines the process by which cities acquire new cells. This class specifies
    /// the amount of culture or gold required to acquire a new cell, what cells are
    /// available for acquisition, and what cell a city should actively pursue.
    /// </summary>
    public interface IBorderExpansionLogic {

        #region methods

        /// <summary>
        /// Returns a collection containing all cells that a given city is permitted
        /// to acquire at this time.
        /// </summary>
        /// <param name="city">The city in question.</param>
        /// <returns>All cells that city could acquire.</returns>
        IEnumerable<IHexCell> GetAllCellsAvailableToCity(ICity city);

        /// <summary>
        /// Returns the acquirable cell that best suits the given city's current resource
        /// focus.
        /// </summary>
        /// <param name="city">The city in question.</param>
        /// <returns>The best acquirable cell for that city.</returns>
        IHexCell GetNextCellToPursue(ICity city);

        /// <summary>
        /// Determines whether the given cell is a available for acquisition by the given city.
        /// </summary>
        /// <param name="city">The city in question.</param>
        /// <param name="cell">The tile in question.</param>
        /// <returns>Whether the cell is available for acquisition by the city.</returns>
        bool IsCellAvailable(ICity city, IHexCell cell);

        /// <summary>
        /// Determines the cost, in culture, for the given city to acquire the given cell.
        /// </summary>
        /// <param name="city">The city in question.</param>
        /// <param name="cell">The cell in question.</param>
        /// <returns>The culture cost of the acquisition.</returns>
        int GetCultureCostOfAcquiringCell(ICity city, IHexCell cell);

        /// <summary>
        /// Determines the cost, in gold, for the given city to acquire the given cell.
        /// </summary>
        /// <param name="city">The city in question.</param>
        /// <param name="cell">The cell in question.</param>
        /// <returns>The gold cost of the acquisition.</returns>
        int GetGoldCostOfAcquiringCell(ICity city, IHexCell cell);

        #endregion

    }

}
