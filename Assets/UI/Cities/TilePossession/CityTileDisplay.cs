using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;
using UniRx;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Territory;

namespace Assets.UI.Cities.TilePossession {

    public class CityTileDisplay : CityDisplayBase {

        #region instance fields and properties

        private List<IWorkerSlotDisplay> InstantiatedDisplays = new List<IWorkerSlotDisplay>();

        private ITilePossessionCanon PossessionCanon;

        private IWorkerSlotDisplay SlotDisplayPrefab;

        private ICityUIConfig Config;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(ITilePossessionCanon possessionCanon, IWorkerSlotDisplay slotDisplayPrefab,
            ICityUIConfig config) {

            PossessionCanon    = possessionCanon;
            SlotDisplayPrefab  = slotDisplayPrefab;
            Config             = config;
        }

        #region from CityDisplayBase

        protected override void DisplayCity(ICity city) {
            foreach(var display in InstantiatedDisplays) {
                display.gameObject.SetActive(false);
            }

            int slotDisplayIndex = 0;

            foreach(var tile in PossessionCanon.GetTilesOfCity(city)) {
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
