using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.Cities.Buildings {

    /// <summary>
    /// the standard implementation for IBuildingPossessionCanon.
    /// </summary>
    public class BuildingPossessionCanon : IBuildingPossessionCanon {

        #region instance fields and properties

        private DictionaryOfLists<ICity, IBuilding> BuildingsInCity = new DictionaryOfLists<ICity, IBuilding>();

        private Dictionary<IBuilding, ICity> CityOfBuilding = new Dictionary<IBuilding, ICity>();

        #endregion

        #region instance methods

        #region from IBuildingPossessionCanon

        /// <inheritdoc/>
        public bool CanPlaceBuildingInCity(IBuilding building, ICity city) {
            if(building == null) {
                throw new ArgumentNullException("building");
            }else if(city == null) {
                throw new ArgumentNullException("city");
            }

            return !CityOfBuilding.ContainsKey(building);
        }

        /// <inheritdoc/>
        public ReadOnlyCollection<IBuilding> GetBuildingsInCity(ICity city) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }

            return BuildingsInCity[city].AsReadOnly();
        }

        /// <inheritdoc/>
        public ICity GetCityOfBuilding(IBuilding building) {
            if(building == null) {
                throw new ArgumentNullException("building");
            }

            ICity retval;
            CityOfBuilding.TryGetValue(building, out retval);
            return retval;
        }

        /// <inheritdoc/>
        public void PlaceBuildingInCity(IBuilding building, ICity city) {
            if(!CanPlaceBuildingInCity(building, city)) {
                throw new InvalidOperationException("CanPlaceBuildingInCity must return true on the given arguments");
            }

            BuildingsInCity.AddElementToList(city, building);
            CityOfBuilding[building] = city;
        }

        /// <inheritdoc/>
        public void RemoveBuildingFromCurrentCity(IBuilding building) {
            if(building == null) {
                throw new ArgumentNullException("building");
            }

            var currentCity = GetCityOfBuilding(building);
            if(currentCity != null) {
                BuildingsInCity[currentCity].Remove(building);
            }

            CityOfBuilding[building] = null;
        }

        #endregion

        #endregion
        
    }

}
