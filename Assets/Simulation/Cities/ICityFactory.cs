using System;
using System.Collections.ObjectModel;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Cities {

    /// <summary>
    /// The base interface for all city factories, which construct cities belonging to
    /// a particular city on a particular cell.
    /// </summary>
    public interface ICityFactory : IFactory<IHexCell, ICivilization, ICity> {

        #region properties

        /// <summary>
        /// All the cities that were created or are recognized by the factory.
        /// </summary>
        ReadOnlyCollection<ICity> AllCities { get; }

        #endregion

    }

}