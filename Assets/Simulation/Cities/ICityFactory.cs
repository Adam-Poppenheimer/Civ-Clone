using System;
using System.Collections.ObjectModel;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Cities {

    public interface ICityFactory : IFactory<IHexCell, ICivilization, ICity> {

        #region properties

        ReadOnlyCollection<ICity> AllCities { get; }

        #endregion

    }

}