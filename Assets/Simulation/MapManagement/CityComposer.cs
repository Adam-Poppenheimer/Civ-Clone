using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Cities.Production;
using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;
using Assets.Simulation.Units;

namespace Assets.Simulation.MapManagement {

    public class CityComposer : ICityComposer {

        #region instance fields and properties

        private IHexGrid                                      Grid;
        private ICityFactory                                  CityFactory;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private IPossessionRelationship<IHexCell, ICity>      CityLocationCanon;
        private ICivilizationFactory                          CivilizationFactory;
        private IEnumerable<IBuildingTemplate>                AvailableBuildingTemplates;
        private IEnumerable<IUnitTemplate>                    AvailableUnitTemplates;
        private IProductionProjectFactory                     ProjectFactory;
        private IFreeBuildingApplier                          FreeBuildingApplier;

        #endregion

        #region constructors

        [Inject]
        public CityComposer(
            IHexGrid grid,
            ICityFactory cityFactory,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon,
            ICivilizationFactory civilizationFactory,
            List<IBuildingTemplate> availableBuildingTemplates, 
            [Inject(Id = "Available Unit Templates")] IEnumerable<IUnitTemplate> availableUnitTemplates,
            IProductionProjectFactory projectFactory,
            IFreeBuildingApplier freeBuildingApplier
        ) {
            Grid                       = grid;
            CityFactory                = cityFactory;
            CityPossessionCanon        = cityPossessionCanon;
            CityLocationCanon          = cityLocationCanon;
            CivilizationFactory        = civilizationFactory;
            AvailableBuildingTemplates = availableBuildingTemplates;
            AvailableUnitTemplates     = availableUnitTemplates;
            ProjectFactory             = projectFactory;
            FreeBuildingApplier        = freeBuildingApplier;
        }

        #endregion

        #region instance methods

        #region from ICityComposer

        public void ClearRuntime(bool immediateMode) {
            foreach(var city in new List<ICity>(CityFactory.AllCities)) {
                CityLocationCanon.ChangeOwnerOfPossession(city, null);

                city.Destroy(immediateMode);
            }

            FreeBuildingApplier.Clear();
        }

        public void ComposeCities(SerializableMapData mapData) {
            mapData.Cities = new List<SerializableCityData>();

            foreach(var city in CityFactory.AllCities) {
                var cityData = new SerializableCityData() {
                    Name             = city.Name,
                    Location         = CityLocationCanon.GetOwnerOfPossession(city).Coordinates,
                    Owner            = CityPossessionCanon.GetOwnerOfPossession(city).Template.Name,
                    Population       = city.Population,
                    FoodStockpile    = city.FoodStockpile,
                    CultureStockpile = city.CultureStockpile,
                    YieldFocus       = city.YieldFocus,
                    Hitpoints        = city.CombatFacade.CurrentHitpoints,
                    CurrentMovement  = city.CombatFacade.CurrentMovement
                };

                var activeProject = city.ActiveProject;

                if(activeProject != null) {
                    cityData.ActiveProject = new SerializableProjectData() {
                        BuildingToConstruct = activeProject.BuildingToConstruct != null ? activeProject.BuildingToConstruct.name : null,
                        UnitToConstruct     = activeProject.UnitToConstruct     != null ? activeProject.UnitToConstruct    .name : null,

                        Progress = activeProject.Progress
                    };
                }

                cityData.AppliedFreeBuildings = FreeBuildingApplier.GetTemplatesAppliedToCity(city)
                                                                       .Select(template => template.name)
                                                                       .ToList();

                mapData.Cities.Add(cityData);
            }
        }

        public void DecomposeCities(SerializableMapData mapData) {
            foreach(var cityData in mapData.Cities) {

                var owner = CivilizationFactory.AllCivilizations.Where(civ => civ.Template.Name.Equals(cityData.Owner)).FirstOrDefault();
                var location = Grid.GetCellAtCoordinates(cityData.Location);

                var newCity = CityFactory.Create(location, owner, cityData.Name);

                newCity.Population                    = cityData.Population;
                newCity.FoodStockpile                 = cityData.FoodStockpile;
                newCity.CultureStockpile              = cityData.CultureStockpile;
                newCity.YieldFocus                    = cityData.YieldFocus;
                newCity.CombatFacade.CurrentHitpoints = cityData.Hitpoints;
                newCity.CombatFacade.CurrentMovement  = cityData.CurrentMovement;

                DecomposeActiveProject   (cityData, newCity);
                DecomposeAppliedTemplates(cityData, newCity);
            }
        }

        #endregion

        private void DecomposeActiveProject(SerializableCityData cityData, ICity newCity) {
            if(cityData.ActiveProject != null) {
                if(cityData.ActiveProject.BuildingToConstruct != null) {

                    var buildingTemplate = AvailableBuildingTemplates.Where(
                        template => template.name.Equals(cityData.ActiveProject.BuildingToConstruct)
                    ).First();
                        
                    newCity.ActiveProject = ProjectFactory.ConstructProject(buildingTemplate);

                }else {
                    var unitTemplate = AvailableUnitTemplates.Where(
                        template => template.name.Equals(cityData.ActiveProject.UnitToConstruct)
                    ).First();

                    newCity.ActiveProject = ProjectFactory.ConstructProject(unitTemplate);
                }

                newCity.ActiveProject.Progress = cityData.ActiveProject.Progress;
            }
        }

        private void DecomposeAppliedTemplates(SerializableCityData cityData, ICity newCity) {
            if(cityData.AppliedFreeBuildings != null) {
                var appliedBuildings = new List<IBuildingTemplate>();

                foreach(var templateName in cityData.AppliedFreeBuildings) {
                    var templateOfName = AvailableBuildingTemplates.Where(
                        template => template.name.Equals(templateName)
                    ).FirstOrDefault();

                    if(templateOfName == null) {
                        throw new InvalidOperationException(string.Format(
                            "CityData.AppliedBuildingTemplates contained invalid building name {0}", templateName
                        ));
                    }

                    appliedBuildings.Add(templateOfName);
                }

                FreeBuildingApplier.OverrideTemplatesAppliedToCity(newCity, appliedBuildings);
            }
        }

        #endregion

    }

}
