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
using Assets.Simulation.Units;

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

        private DiContainer                                   Container;
        private GameObject                                    CityPrefab;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private IHexGrid                                      Grid;
        private IPossessionRelationship<ICity, IHexCell>      CellPossessionCanon;
        private IUnitFactory                                  UnitFactory;
        private IPossessionRelationship<IHexCell, ICity>      CityLocationCanon;
        private ICellModificationLogic                        CellModificationLogic;
        private Transform                                     CityContainer;

        #endregion

        #region constructors

        [Inject]
        public CityFactory(DiContainer container, [Inject(Id = "City Prefab")] GameObject cityPrefab,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon, IHexGrid grid, 
            IPossessionRelationship<ICity, IHexCell> cellPossessionCanon, CitySignals citySignals,
            IUnitFactory unitFactory, IPossessionRelationship<IHexCell, ICity> cityLocationCanon,
            ICellModificationLogic cellModificationLogic,
            [Inject(Id = "City Container")] Transform cityContainer
        ){
            Container             = container;
            CityPrefab            = cityPrefab;
            CityPossessionCanon   = cityPossessionCanon;
            Grid                  = grid;
            CellPossessionCanon   = cellPossessionCanon;
            UnitFactory           = unitFactory;
            CityLocationCanon     = cityLocationCanon;
            CellModificationLogic = cellModificationLogic;
            CityContainer         = cityContainer;

            if(citySignals != null) {
                citySignals.CityBeingDestroyedSignal.Subscribe(OnCityBeingDestroyed);
            }
        }

        #endregion

        #region instance methods

        #region from ICityFactory

        public ICity Create(IHexCell location, ICivilization owner, string name) {
            if(location == null) {
                throw new ArgumentNullException("location");
            }else if(owner == null) {
                throw new ArgumentNullException("owner");
            }

            var newCityGameObject = GameObject.Instantiate(CityPrefab);
            Container.InjectGameObject(newCityGameObject);

            newCityGameObject.transform.position = location.AbsolutePosition;
            newCityGameObject.name = string.Format("{0} (City)", name);
            newCityGameObject.transform.SetParent(CityContainer, true);

            var newCity = newCityGameObject.GetComponent<City>();

            newCity.Population = 1;

            CityLocationCanon.ChangeOwnerOfPossession(newCity, location);

            location.SuppressSlot = true;
            
            CellModificationLogic.ChangeVegetationOfCell(location, CellVegetation.None);

            if(CityPossessionCanon.CanChangeOwnerOfPossession(newCity, owner)) {
                CityPossessionCanon.ChangeOwnerOfPossession(newCity, owner);
            }else {
                throw new CityCreationException("Cannot assign the newly created city to its intended civilization");
            }

            var combatantTemplate = Container.Instantiate<CityCombatantTemplate>(new object[]{ newCity });
            newCity.CombatFacade = UnitFactory.BuildUnit(location, combatantTemplate, owner);

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

            newCity.YieldFocus = YieldFocusType.TotalYield;
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
            city.CombatFacade.Destroy();
            allCities.Remove(city);
        }

        #endregion

    }

}
