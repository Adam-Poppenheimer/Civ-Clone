using System;

namespace Assets.Cities.UI {

    public interface ICityGrowthDisplay {

        #region properties

        ICity CityToDisplay { get; set; }

        #endregion

        #region methods

        void Refresh();

        #endregion

    }

}