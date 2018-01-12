using System;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Cities {

    /// <summary>
    /// Determines whether a cell is a valid location for a city or not.
    /// </summary>
    /// <remarks>
    /// This class is intended to handle the gameplay logic that restricts
    /// city placement. It doesn't indicate whether CityFactory is capable
    /// of creating a particular city.
    /// </remarks>
    public interface ICityValidityLogic {

        #region methods

        /// <summary>
        /// Determines whether the argued cell is a valid location for a city
        /// given certain gameplay rules.
        /// </summary>
        /// <param name="cell">The cell whose validity is being checked</param>
        /// <returns>Whether the cell is a valid location for a new city</returns>
        bool IsCellValidForCity(IHexCell cell);

        #endregion

    }

}