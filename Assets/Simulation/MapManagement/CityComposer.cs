using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;
using Assets.Simulation.Units;

namespace Assets.Simulation.MapManagement {

    public class CityComposer {

        #region instance fields and properties

        private IHexGrid                                      Grid;
        private ICityFactory                                  CityFactory;
        private IBuildingFactory                              BuildingFactory;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private IPossessionRelationship<ICity, IBuilding>     BuildingPossessionCanon;
        private ICivilizationFactory                          CivilizationFactory;
        private IEnumerable<IBuildingTemplate>                AvailableBuildingTemplates;
        private IEnumerable<IUnitTemplate>                    AvailableUnitTemplates;

        #endregion

        #region constructors

        [Inject]
        public CityComposer(
            IHexGrid grid, ICityFactory cityFactory, IBuildingFactory buildingFactory,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IPossessionRelationship<ICity, IBuilding> buildingPossessionCanon,
            ICivilizationFactory civilizationFactory, List<IBuildingTemplate> availableBuildingTemplates, 
            [Inject(Id = "Available Unit Templates")] IEnumerable<IUnitTemplate> availableUnitTemplates
        ) {
            Grid                       = grid;
            CityFactory                = cityFactory;
            BuildingFactory            = buildingFactory;
            CityPossessionCanon        = cityPossessionCanon;
            BuildingPossessionCanon    = buildingPossessionCanon;
            CivilizationFactory        = civilizationFactory;
            AvailableBuildingTemplates = availableBuildingTemplates;
            AvailableUnitTemplates     = availableUnitTemplates;
        }

        #endregion

        #region instance methods

        public void ClearRuntime() {
            foreach(var city in new List<ICity>(CityFactory.AllCities)) {
                GameObject.DestroyImmediate(city.gameObject);
            }
        }

        public void ComposeCities(SerializableMapData mapData) {
            mapData.Cities = new List<SerializableCityData>();

            foreach(var city in CityFactory.AllCities) {
                var cityData = new SerializableCityData() {
                    Location         = city.Location.Coordinates,
                    Owner            = CityPossessionCanon.GetOwnerOfPossession(city).Name,
                    Population       = city.Population,
                    FoodStockpile    = city.FoodStockpile,
                    CultureStockpile = city.CultureStockpile,
                    ResourceFocus    = city.ResourceFocus,
                    CurrentHealth    = city.CombatFacade.Health,
                    CurrentMovement  = city.CombatFacade.CurrentMovement
                };

                var activeProject = city.ActiveProject;

                if(activeProject != null) {
                    cityData.ActiveProject = new SerializableProjectData() {
                        BuildingToConstruct = activeProject.BuildingToConstruct != null ? activeProject.BuildingToConstruct.name : null,
                        UnitToConstruct     = activeProject.UnitToConstruct     != null ? activeProject.UnitToConstruct    .Name : null,

                        Progress = activeProject.Progress
                    };
                }

                var buildingsInCity = BuildingPossessionCanon.GetPossessionsOfOwner(city);

                cityData.Buildings = new List<SerializableBuildingData>();

                foreach(var building in buildingsInCity) {
                    var newBuildingData = new SerializableBuildingData() {
                        Template = building.Template.name,
                        IsSlotLocked   = building.Slots.Select(slot => slot.IsLocked  ).ToList(),
                        IsSlotOccupied = building.Slots.Select(slot => slot.IsOccupied).ToList()
                    };

                    cityData.Buildings.Add(newBuildingData);
                }

                mapData.Cities.Add(cityData);
            }
        }

        public void DecomposeCities(SerializableMapData mapData) {
            foreach(var cityData in mapData.Cities) {

                var owner = CivilizationFactory.AllCivilizations.Where(civ => civ.Name.Equals(cityData.Owner)).FirstOrDefault();
                var location = Grid.GetCellAtCoordinates(cityData.Location);

                var newCity = CityFactory.Create(location, owner);

                newCity.Population                   = cityData.Population;
                newCity.FoodStockpile                = cityData.FoodStockpile;
                newCity.CultureStockpile             = cityData.CultureStockpile;
                newCity.ResourceFocus                = cityData.ResourceFocus;
                newCity.CombatFacade.Health          = cityData.CurrentHealth;
                newCity.CombatFacade.CurrentMovement = cityData.CurrentMovement;

                if(cityData.ActiveProject != null) {
                    if(cityData.ActiveProject.BuildingToConstruct != null) {

                        var buildingTemplate = AvailableBuildingTemplates.Where(
                            template => template.name.Equals(cityData.ActiveProject.BuildingToConstruct)
                        ).First();
                        
                        newCity.SetActiveProductionProject(buildingTemplate);

                    }else {
                        var unitTemplate = AvailableUnitTemplates.Where(
                            template => template.Name.Equals(cityData.ActiveProject.UnitToConstruct)
                        ).First();

                        newCity.SetActiveProductionProject(unitTemplate);
                    }

                    newCity.ActiveProject.Progress = cityData.ActiveProject.Progress;
                }

                foreach(var buildingData in cityData.Buildings) {
                    var templateToBuild = AvailableBuildingTemplates.Where(template => template.name.Equals(buildingData.Template)).First();

                    var newBuilding = BuildingFactory.Create(templateToBuild, newCity);

                    for(int i = 0; i < newBuilding.Slots.Count; i++) {
                        var slot = newBuilding.Slots[i];
                        
                        slot.IsOccupied = buildingData.IsSlotOccupied[i];
                        slot.IsLocked   = buildingData.IsSlotLocked  [i];
                    }
                }
            }
        }

        #endregion

    }

}
