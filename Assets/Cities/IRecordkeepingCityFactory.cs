using System;
using System.Collections.ObjectModel;

using Zenject;

namespace Assets.Cities {

    public interface IRecordkeepingCityFactory : IFactory<ICity> {

        #region properties

        ReadOnlyCollection<ICity> AllCities { get; }

        #endregion

    }

}