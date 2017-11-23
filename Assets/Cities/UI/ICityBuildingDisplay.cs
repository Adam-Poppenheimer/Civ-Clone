using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Cities.Buildings;

namespace Assets.Cities.UI {

    public interface ICityBuildingDisplay {

        #region methods

        void DisplayBuildings(IEnumerable<IBuilding> buildings);

        #endregion

    }

}
