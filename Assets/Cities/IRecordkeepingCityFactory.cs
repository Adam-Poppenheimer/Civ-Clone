using System;
using System.Collections.ObjectModel;

using Zenject;

using Assets.GameMap;

namespace Assets.Cities {

    public interface IRecordkeepingCityFactory : IFactory<IMapTile, ICity> {

        #region properties

        ReadOnlyCollection<ICity> AllCities { get; }

        #endregion

    }

}