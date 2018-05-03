using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.HexMap;
using Assets.Simulation.WorkerSlots;

namespace Assets.Simulation.MapManagement {

    public class BuildingComposer {

        #region instance fields and properties

        private IHexGrid                                  Grid;
        private IBuildingFactory                          BuildingFactory;
        private IPossessionRelationship<ICity, IBuilding> BuildingPossessionCanon;
        private IPossessionRelationship<IHexCell, ICity>  CityLocationCanon;
        private IEnumerable<IBuildingTemplate>            AvailableBuildingTemplates;

        #endregion

        #region constructors

        [Inject]
        public BuildingComposer(
            IHexGrid grid, IBuildingFactory buildingFactory,
            IPossessionRelationship<ICity, IBuilding> buildingPossessionCanon,
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon,
            [Inject(Id = "Available Building Templates")] IEnumerable<IBuildingTemplate> availableBuildingTemplates
        ){
            Grid                       = grid;
            BuildingFactory            = buildingFactory;
            BuildingPossessionCanon    = buildingPossessionCanon;
            CityLocationCanon          = cityLocationCanon;
            AvailableBuildingTemplates = availableBuildingTemplates;
        }

        #endregion

        #region instance methods

        public void ClearRuntime() {
            foreach(var building in new List<IBuilding>(BuildingFactory.AllBuildings)) {
                BuildingPossessionCanon.ChangeOwnerOfPossession(building, null);
            }
        }

        public void ComposeBuildings(SerializableMapData mapData) {
            mapData.Buildings = new List<SerializableBuildingData>();

            foreach(var building in BuildingFactory.AllBuildings) {
                var buildingOwner = BuildingPossessionCanon.GetOwnerOfPossession(building);
                var ownerLocation = CityLocationCanon.GetOwnerOfPossession(buildingOwner);

                var buildingData = new SerializableBuildingData();

                buildingData.Template       = building.Template.name;
                buildingData.CityLocation   = ownerLocation.Coordinates;
                buildingData.IsSlotLocked   = building.Slots.Select(slot => slot.IsLocked)  .ToList();
                buildingData.IsSlotOccupied = building.Slots.Select(slot => slot.IsOccupied).ToList();

                mapData.Buildings.Add(buildingData);
            }
        }

        public void DecomposeBuildings(SerializableMapData mapData) {
            foreach(var buildingData in mapData.Buildings) {
                var templateToBuild = AvailableBuildingTemplates.Where(template => template.name.Equals(buildingData.Template)).First();

                var cellAtCoords   = Grid.GetCellAtCoordinates(buildingData.CityLocation);
                var cityAtLocation = CityLocationCanon.GetPossessionsOfOwner(cellAtCoords).First();

                var newBuilding = BuildingFactory.Create(templateToBuild, cityAtLocation);

                for(int i = 0; i < newBuilding.Slots.Count; i++) {
                    var slot = newBuilding.Slots[i];

                    slot.IsOccupied = buildingData.IsSlotOccupied[i];
                    slot.IsLocked   = buildingData.IsSlotLocked  [i];
                }
            }
        }

        #endregion

    }

}
