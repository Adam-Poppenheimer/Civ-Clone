using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Cities.Distribution;
using Assets.Simulation.Cities.Territory;

namespace Assets.Simulation.Cities {

    /// <summary>
    /// The standard implementation for ICityFactory.
    /// </summary>
    public class CityFactory : ICityFactory, IValidatable {

        #region instance fields and properties

        #region from ICityFactory

        /// <inheritdoc/>
        public ReadOnlyCollection<ICity> AllCities {
            get { return allCities.AsReadOnly(); }
        }
        private List<ICity> allCities = new List<ICity>();

        #endregion

        private DiContainer Container;

        private GameObject CityPrefab;

        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        private IHexGrid Grid;

        private IPossessionRelationship<ICity, IHexCell> CellPossessionCanon;

        #endregion

        #region constructors

        /// <summary>
        /// Constructs a factory capable of creating properly-integrated cities.
        /// </summary>
        /// <param name="container">The container used to inject dependencies into newly-created factories.</param>
        /// <param name="cityPrefab">The prefab used to instantiate new cities.</param>
        /// <param name="cityPossessionCanon">The canon used to assign new cities to their proper owners.</param>
        /// <param name="grid">The HexGrid used.</param>
        /// <param name="cellPossessionCanon">The canon used to assign adjacent cells to the city</param>
        [Inject]
        public CityFactory(DiContainer container, [Inject(Id = "City Prefab")] GameObject cityPrefab,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon, IHexGrid grid, 
            IPossessionRelationship<ICity, IHexCell> cellPossessionCanon, CitySignals citySignals
        ){
            Container           = container;
            CityPrefab          = cityPrefab;
            CityPossessionCanon = cityPossessionCanon;
            Grid                = grid;
            CellPossessionCanon = cellPossessionCanon;

            citySignals.CityBeingDestroyedSignal.Subscribe(OnCityBeingDestroyed);
        }

        #endregion

        #region instance methods

        #region from ICityFactory

        /// <summary>
        /// Creates a new city at the argued location belonging to the argued civilization.
        /// </summary>
        /// <param name="location">The location to place the city</param>
        /// <param name="owner">The owner the city will belong to</param>
        /// <returns>A fully-instantiated anc configured city with the correct location and owner</returns>
        /// <remarks>
        /// This method makes sure that the created city has the correct population, location,
        /// owner, starting tiles, and resource focus. It also suppresses the slot of the argued location,
        /// performs distribution on the city, adds it to the AllCities collection, and refreshes the city's
        /// location.
        /// </remarks>
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

            if(CellPossessionCanon.CanChangeOwnerOfPossession(location, newCity)) {
                CellPossessionCanon.ChangeOwnerOfPossession(location, newCity);
            }else {
                throw new CityCreationException("Cannot assign the given location to the newly created city");
            }
            
            foreach(var neighbor in Grid.GetNeighbors(location)) {
                if(CellPossessionCanon.CanChangeOwnerOfPossession(neighbor, newCity)) {
                    CellPossessionCanon.ChangeOwnerOfPossession(neighbor, newCity);
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

            location.RefreshSelfOnly();
            return newCity;
        }

        #endregion

        #region from IValidatable        

        /// <inheritdoc/>
        public void Validate() {
            var newCityGameObject = GameObject.Instantiate(CityPrefab);
            Container.InjectGameObject(newCityGameObject);
        }

        #endregion

        private void OnCityBeingDestroyed(ICity city) {
            allCities.Remove(city);
        }

        #endregion

    }

}
