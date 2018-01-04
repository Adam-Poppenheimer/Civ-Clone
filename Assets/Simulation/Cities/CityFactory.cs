using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Cities.Distribution;
using Assets.Simulation.Cities.Territory;

namespace Assets.Simulation.Cities {

    public class CityFactory : ICityFactory , IInitializable, IValidatable {

        #region instance fields and properties

        #region from ICityFactory

        public ReadOnlyCollection<ICity> AllCities {
            get { return allCities.AsReadOnly(); }
        }
        private List<ICity> allCities = new List<ICity>();

        #endregion

        private DiContainer Container;

        private GameObject CityPrefab;

        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        private IHexGrid Map;

        private ITilePossessionCanon TilePossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public CityFactory(DiContainer container, [Inject(Id = "City Prefab")] GameObject cityPrefab,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon, IHexGrid map, 
            ITilePossessionCanon tilePossessionCanon
        ){
            Container           = container;
            CityPrefab          = cityPrefab;
            CityPossessionCanon = cityPossessionCanon;
            Map                 = map;
            TilePossessionCanon = tilePossessionCanon;
        }

        #endregion

        #region instance methods

        #region from ICityFactory

        public ICity Create(IHexCell location, ICivilization owner){
            if(location == null) {
                throw new ArgumentNullException("location");
            }else if(owner == null) {
                throw new ArgumentNullException("owner");
            }

            var newCityGameObject = GameObject.Instantiate(CityPrefab);
            Container.InjectGameObject(newCityGameObject);

            newCityGameObject.transform.SetParent(location.transform, false);

            var newCity = newCityGameObject.GetComponent<City>();
            newCity.Population = 1;
            newCity.Location = location;
            location.SuppressSlot = true;

            if(TilePossessionCanon.CanChangeOwnerOfTile(location, newCity)) {
                TilePossessionCanon.ChangeOwnerOfTile(location, newCity);
            }else {
                throw new CityCreationException("Cannot assign the given location to the newly created city");
            }
            
            foreach(var neighbor in Map.GetNeighbors(location)) {
                if(TilePossessionCanon.CanChangeOwnerOfTile(neighbor, newCity)) {
                    TilePossessionCanon.ChangeOwnerOfTile(neighbor, newCity);
                }                
            }
            
            if(CityPossessionCanon.CanChangeOwnerOfPossession(newCity, owner)) {
                CityPossessionCanon.ChangeOwnerOfPossession(newCity, owner);
            }else {
                throw new CityCreationException("Cannot assign the newly created city to its intended civilization");
            }
            

            newCity.ResourceFocus = ResourceFocusType.TotalYield;
            newCity.PerformDistribution();

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
