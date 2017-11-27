using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Simulation.Cities.Buildings {

    public interface IBuildingPossessionCanon {

        #region methods

        ReadOnlyCollection<IBuilding> GetBuildingsInCity(ICity city);

        ICity GetCityOfBuilding(IBuilding building);

        bool CanPlaceBuildingInCity(IBuilding building, ICity city);

        void PlaceBuildingInCity(IBuilding building, ICity city);

        void RemoveBuildingFromCurrentCity(IBuilding building);

        #endregion

    }

}
