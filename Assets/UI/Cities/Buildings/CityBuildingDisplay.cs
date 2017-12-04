using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using UniRx;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;

namespace Assets.UI.Cities.Buildings {

    public class CityBuildingDisplay : CityDisplayBase {

        #region instance fields and properties

        [SerializeField] private Transform BuildingDisplayParent;

        private List<BuildingDisplay> InstantiatedBuildingDisplays = new List<BuildingDisplay>();

        private IBuildingPossessionCanon PossessionCanon;

        private BuildingDisplay.Factory DisplayFactory;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(IBuildingPossessionCanon possessionCanon, BuildingDisplay.Factory displayFactory) {
            PossessionCanon = possessionCanon;
            DisplayFactory = displayFactory;
        }

        #region from CityDisplayBase

        public override void Refresh() {
            if(ObjectToDisplay == null) {
                return;
            }
            DisplayBuildings(PossessionCanon.GetBuildingsInCity(ObjectToDisplay));
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

        private BuildingDisplay GetBuildingDisplay(int index) {
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
