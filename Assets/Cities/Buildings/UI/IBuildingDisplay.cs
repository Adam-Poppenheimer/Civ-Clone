using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Cities.UI;

namespace Assets.Cities.Buildings.UI {

    public interface IBuildingDisplay {

        #region properties

        GameObject gameObject { get; }

        #endregion

        #region methods

        void DisplayBuilding(IBuilding building, ICityUIConfig config);

        #endregion

    }

}
