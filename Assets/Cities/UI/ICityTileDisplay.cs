using System;

namespace Assets.Cities.UI {

    public interface ICityTileDisplay {

        #region properties

        ICity CityToDisplay { get; set; }

        #endregion

        #region methods

        void Refresh();

        #endregion

    }

}