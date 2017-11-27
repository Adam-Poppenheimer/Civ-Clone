using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.Cities.Buildings;

namespace Assets.UI.Cities.Buildings {

    public interface IBuildingDisplay {

        #region properties

        GameObject gameObject { get; }

        #endregion

        #region methods

        void DisplayBuilding(IBuilding building, ICityUIConfig config);

        #endregion

    }

}
