using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using UniRx;

using Assets.Simulation;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;

namespace Assets.UI.Cities.Buildings {

    public class CityBuildingDisplay : CityDisplayBase {

        #region instance fields and properties

        [InjectOptional(Id = "Building Display Parent")]
        private Transform BuildingDisplayParent {
            get { return _buildingDisplayParent; }
            set {
                if(value != null) {
                    _buildingDisplayParent = value;
                }
            }
        }
        [SerializeField] private Transform _buildingDisplayParent;

        private List<IBuildingDisplay> InstantiatedBuildingDisplays = new List<IBuildingDisplay>();

        private IPossessionRelationship<ICity, IBuilding> PossessionCanon;

        private BuildingDisplayFactory DisplayFactory;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(IPossessionRelationship<ICity, IBuilding> possessionCanon, BuildingDisplayFactory displayFactory) {
            PossessionCanon = possessionCanon;
            DisplayFactory = displayFactory;
        }

        #region from CityDisplayBase

        public override void Refresh() {
            if(ObjectToDisplay == null) {
                return;
            }
            DisplayBuildings(PossessionCanon.GetPossessionsOfOwner(ObjectToDisplay));
        }

        #endregion

        private void DisplayBuildings(IEnumerable<IBuilding> buildings) {
            foreach(var buildingDisplay in InstantiatedBuildingDisplays) {
                buildingDisplay.gameObject.SetActive(false);
            }

            int buildingDisplayIndex = 0;
            foreach(var building in buildings) {   
                             
                var buildingDisplay = GetBuildingDisplay(buildingDisplayIndex++);

                buildingDisplay.BuildingToDisplay = building;

                buildingDisplay.gameObject.SetActive(true);
                buildingDisplay.Refresh();
            }
        }

        private IBuildingDisplay GetBuildingDisplay(int index) {
            if(InstantiatedBuildingDisplays.Count == index) {
                var newDisplay = DisplayFactory.Create();

                newDisplay.gameObject.transform.SetParent(BuildingDisplayParent, false);

                InstantiatedBuildingDisplays.Add(newDisplay);
            }
            return InstantiatedBuildingDisplays[index];
        }

        #endregion
        
    }
}
