using System;
using System.Collections.ObjectModel;

using Zenject;

using Assets.Simulation.GameMap;

namespace Assets.Simulation.Cities {

    public interface IRecordkeepingCityFactory : IFactory<IMapTile, ICity> {

        #region properties

        ReadOnlyCollection<ICity> AllCities { get; }

        #endregion

    }

}