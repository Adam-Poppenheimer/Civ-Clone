using System;

using UnityEngine;

using Zenject;

using Assets.Simulation.Cities.Buildings;

namespace Assets.UI.Cities.Buildings {

    public interface IBuildingDisplay {

        #region properties

        GameObject gameObject { get; }

        IBuilding BuildingToDisplay { get; set; }

        #endregion

        #region methods

        void Refresh();

        #endregion

    }

    public class BuildingDisplayFactory : Factory<IBuildingDisplay> { }

}