using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

namespace Assets.Cities.UI {

    public class CityTileDisplay : MonoBehaviour, ICityTileDisplay {

        #region instance fields and properties

        #region from ITileDistributionDisplay

        public ICity CityToDisplay { get; set; }

        #endregion

        private List<IWorkerSlotDisplay> InstantiatedDisplays = new List<IWorkerSlotDisplay>();

        private ITilePossessionCanon PossessionCanon;

        private IWorkerSlotDisplay SlotDisplayPrefab;

        private ICityUIConfig Config;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(ITilePossessionCanon possessionCanon, IWorkerSlotDisplay slotDisplayPrefab,
            ICityUIConfig config) {
            PossessionCanon = possessionCanon;
            SlotDisplayPrefab = slotDisplayPrefab;
            Config = config;
        }

        #region from ITileDistributionDisplay

        public void Refresh() {
            if(CityToDisplay == null) {
                return;
            }

            foreach(var display in InstantiatedDisplays) {
                display.gameObject.SetActive(false);
            }

            int slotDisplayIndex = 0;

            foreach(var tile in PossessionCanon.GetTilesOfCity(CityToDisplay)) {
                if(tile.SuppressSlot) {
                    continue;
                }

                var slotDisplay = GetNextSlotDisplay(slotDisplayIndex++);

                slotDisplay.transform.position = Camera.main.WorldToScreenPoint(tile.transform.position);
                slotDisplay.DisplayOccupationStatus(tile.WorkerSlot.IsOccupied, Config);

                slotDisplay.gameObject.SetActive(true);
            }
        }

        #endregion

        private IWorkerSlotDisplay GetNextSlotDisplay(int currentIndex) {
            if(currentIndex >= InstantiatedDisplays.Count) {
                var newDisplayObject = GameObject.Instantiate(SlotDisplayPrefab.gameObject);

                newDisplayObject.transform.SetParent(transform, false);
                newDisplayObject.SetActive(false);

                InstantiatedDisplays.Add(newDisplayObject.GetComponent<IWorkerSlotDisplay>());
            }

            return InstantiatedDisplays[currentIndex];
        }

        #endregion

    }

}
