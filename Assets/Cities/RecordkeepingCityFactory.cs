using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Cities {

    public class RecordkeepingCityFactory : IRecordkeepingCityFactory , IInitializable, IValidatable {

        #region instance fields and properties

        public ReadOnlyCollection<ICity> AllCities {
            get { return allCities.AsReadOnly(); }
        }
        private List<ICity> allCities = new List<ICity>();

        private DiContainer Container;

        private GameObject CityPrefab;

        #endregion

        #region constructors

        public RecordkeepingCityFactory(DiContainer container, [Inject(Id = "City Prefab")] GameObject cityPrefab) {
            Container = container;
            CityPrefab = cityPrefab;
        }

        #endregion

        #region instance methods

        #region from IFactory<ICity>
        public ICity Create() {
            var newCityGameObject = GameObject.Instantiate(CityPrefab);
            Container.InjectGameObject(newCityGameObject);

            var newCity = newCityGameObject.GetComponent<ICity>();
            allCities.Add(newCity);
            return newCity;
        }

        #endregion

        #region from IInitializable

        public void Initialize() {
            foreach(var city in GameObject.FindObjectsOfType<City>()) {
                if(!allCities.Contains(city)) {
                    Container.InjectGameObject(city.gameObject);
                    allCities.Add(city);
                }
            }
        }

        #endregion

        #region from IValidatable        

        public void Validate() {
            var newCityGameObject = GameObject.Instantiate(CityPrefab);
            Container.InjectGameObject(newCityGameObject);
        }

        #endregion

        #endregion

    }

}
