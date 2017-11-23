using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Cities.Buildings;
using Assets.Cities.Buildings.UI;

namespace Assets.Cities.UI {

    public class CityBuildingDisplay : MonoBehaviour, ICityBuildingDisplay {

        #region instance fields and properties

        [SerializeField] private GameObject BuildingDisplayPrefab;

        [SerializeField] private Transform BuildingDisplayParent;

        private List<IBuildingDisplay> InstantiatedBuildingDisplays = new List<IBuildingDisplay>();

        private ICityUIConfig Config;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(ICityUIConfig config) {
            Config = config;
        }

        #region from IBuildingDisplay

        public void DisplayBuildings(IEnumerable<IBuilding> buildings) {
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

        #endregion

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
