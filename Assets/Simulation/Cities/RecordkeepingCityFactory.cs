﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.GameMap;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Cities.Distribution;
using Assets.Simulation.Cities.Territory;

namespace Assets.Simulation.Cities {

    public class RecordkeepingCityFactory : IRecordkeepingCityFactory , IInitializable, IValidatable {

        #region instance fields and properties

        #region from IRecordkeepingCityFactory

        public ReadOnlyCollection<ICity> AllCities {
            get { return allCities.AsReadOnly(); }
        }
        private List<ICity> allCities = new List<ICity>();

        #endregion

        private DiContainer Container;

        private GameObject CityPrefab;

        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        private IMapHexGrid Map;

        private ITilePossessionCanon TilePossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public RecordkeepingCityFactory(DiContainer container, [Inject(Id = "City Prefab")] GameObject cityPrefab,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon, IMapHexGrid map, 
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

        #region from IRecordkeepingCityFactory

        public ICity Create(IMapTile location, ICivilization owner){
            var newCityGameObject = GameObject.Instantiate(CityPrefab);
            Container.InjectGameObject(newCityGameObject);

            newCityGameObject.transform.SetParent(location.transform, false);

            var newCity = newCityGameObject.GetComponent<City>();
            newCity.Population = 1;
            newCity.Location = location;
            location.SuppressSlot = true;

            TilePossessionCanon.ChangeOwnerOfTile(location, newCity);
            foreach(var neighbor in Map.GetNeighbors(location)) {
                TilePossessionCanon.ChangeOwnerOfTile(neighbor, newCity);
            }
            
            CityPossessionCanon.ChangeOwnerOfPossession(newCity, owner);

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
