using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Cities.UI {

    public interface ICityExpansionDisplay {

        #region properties

        ICity CityToDisplay { get; set; }

        #endregion

        #region methods

        void Refresh();

        #endregion

    }

}
