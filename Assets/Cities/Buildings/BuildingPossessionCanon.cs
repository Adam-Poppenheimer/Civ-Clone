using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Assets.Cities;

namespace Assets.Cities.Buildings {

    public class BuildingPossessionCanon : IBuildingPossessionCanon {

        #region instance fields and properties



        #endregion

        #region instance methods

        #region from IBuildingPossessionCanon

        public bool CanPlaceBuildingInCity(IBuilding building, ICity city) {
            throw new NotImplementedException();
        }

        public bool CanRemoveBuildingFromCity(IBuilding building, ICity city) {
            throw new NotImplementedException();
        }

        public ReadOnlyCollection<IBuilding> GetBuildingsInCity(ICity city) {
            throw new NotImplementedException();
        }

        public ICity GetCityOfBuilding(IBuilding building) {
            throw new NotImplementedException();
        }

        public void PlaceBuildingInCity(IBuilding building, ICity city) {
            throw new NotImplementedException();
        }

        public void RemoveBuildingFromCurrentCity(IBuilding building) {
            throw new NotImplementedException();
        }

        #endregion

        #endregion
        
    }

}
