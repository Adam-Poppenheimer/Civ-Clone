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

namespace Assets.UI.Cities.Territory {

    public class CityTileDisplay : CityDisplayBase {

        #region instance fields and properties

        private List<IWorkerSlotDisplay> InstantiatedDisplays = new List<IWorkerSlotDisplay>();

        private ICellPossessionCanon PossessionCanon;

        private WorkerSlotDisplayFactory SlotFactory;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(ICellPossessionCanon possessionCanon, WorkerSlotDisplayFactory slotFactory,
            CitySignals signals) {

            PossessionCanon = possessionCanon;
            SlotFactory     = slotFactory;
            
            signals.DistributionPerformedSignal.Listen(OnDistributionPerformed);
        }

        #region Unity message methods

        private void Update() {
            Refresh();
        }

        #endregion

        #region from CityDisplayBase

        public override void Refresh() {
            if(ObjectToDisplay == null) {
                return;
            }

            foreach(var display in InstantiatedDisplays) {
                display.gameObject.SetActive(false);
            }

            int slotDisplayIndex = 0;

            foreach(var tile in PossessionCanon.GetTilesOfCity(ObjectToDisplay)) {
                if(tile.SuppressSlot) {
                    continue;
                }

                var slotDisplay = GetNextSlotDisplay(slotDisplayIndex++);

                slotDisplay.SlotToDisplay = tile.WorkerSlot;

                slotDisplay.gameObject.transform.position = Camera.main.WorldToScreenPoint(tile.transform.position);
                slotDisplay.Refresh();

                slotDisplay.gameObject.SetActive(true);
            }
        }

        #endregion

        private void OnDistributionPerformed(ICity city) {
            if(city == ObjectToDisplay) {
                Refresh();
            }
        }

        private IWorkerSlotDisplay GetNextSlotDisplay(int currentIndex) {
            if(currentIndex >= InstantiatedDisplays.Count) {
                var newDisplay = SlotFactory.Create();

                newDisplay.gameObject.transform.SetParent(transform, false);
                newDisplay.gameObject.SetActive(false);

                InstantiatedDisplays.Add(newDisplay);
            }

            return InstantiatedDisplays[currentIndex];
        }

        #endregion

    }

}
