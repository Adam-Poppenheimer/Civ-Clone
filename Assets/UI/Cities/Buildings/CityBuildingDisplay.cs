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

        [SerializeField] private GameObject BuildingDisplayPrefab;

        [SerializeField] private Transform BuildingDisplayParent;

        private List<IBuildingDisplay> InstantiatedBuildingDisplays = new List<IBuildingDisplay>();

        private ICityUIConfig Config;
        private IBuildingPossessionCanon PossessionCanon;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(ICityUIConfig config, IBuildingPossessionCanon possessionCanon) {
            Config = config;
            PossessionCanon = possessionCanon;
        }

        #region from CityDisplayBase

        protected override void DisplayCity(ICity city) {
            DisplayBuildings(PossessionCanon.GetBuildingsInCity(city));
        }

        #endregion

        private void DisplayBuildings(IEnumerable<IBuilding> buildings) {
            foreach(var buildingDisplay in InstantiatedBuildingDisplays) {
                buildingDisplay.gameObject.SetActive(false);
            }

            int buildingDisplayIndex = 0;
            foreach(var building in buildings) {                
                var buildingDisplay = GetBuildingDisplay(buildingDisplayIndex++);

                buildingDisplay.gameObject.SetActive(true);
                buildingDisplay.DisplayBuilding(building, Config);
            }
        }

        private IBuildingDisplay GetBuildingDisplay(int index) {
            if(InstantiatedBuildingDisplays.Count == index) {
                var newSlotPrefab = Instantiate(BuildingDisplayPrefab);

                var newDisplay = newSlotPrefab.GetComponent<IBuildingDisplay>();
                newDisplay.gameObject.transform.SetParent(BuildingDisplayParent, false);

                InstantiatedBuildingDisplays.Add(newDisplay);
            }
            return InstantiatedBuildingDisplays[index];
        }

        #endregion
        
    }
}
