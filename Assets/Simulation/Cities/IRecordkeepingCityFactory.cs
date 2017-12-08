using System;
using System.Collections.ObjectModel;

using Zenject;

using Assets.Simulation.GameMap;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Cities {

    public interface IRecordkeepingCityFactory : IFactory<IMapTile, ICivilization, ICity> {

        #region properties

        ReadOnlyCollection<ICity> AllCities { get; }

        #endregion

    }

}