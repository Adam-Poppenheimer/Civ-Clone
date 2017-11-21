using System;

namespace Assets.Cities.UI {

    public interface IProductionDisplay {

        #region properties

        ICity CityToDisplay { get; set; }

        #endregion

        #region methods

        void Refresh();

        #endregion

    }

}