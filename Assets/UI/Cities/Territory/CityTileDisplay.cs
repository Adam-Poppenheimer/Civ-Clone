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

        private List<WorkerSlotDisplay> InstantiatedDisplays = new List<WorkerSlotDisplay>();

        private ITilePossessionCanon PossessionCanon;

        private WorkerSlotDisplay.Factory SlotFactory;

        private CitySignals Signals;

        private IDisposable DistributionListeningSubscription;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(ITilePossessionCanon possessionCanon, WorkerSlotDisplay.Factory slotFactory,
            CitySignals signals) {

            PossessionCanon = possessionCanon;
            SlotFactory     = slotFactory;
            Signals         = signals;
        }

        #region from CityDisplayBase

        protected override void DoOnEnable() {
            DistributionListeningSubscription = Signals.DistributionPerformedSignal.AsObservable.Subscribe(OnDistributionPerformed);
        }

        protected override void DoOnDisable() {
            DistributionListeningSubscription.Dispose();
            DistributionListeningSubscription = null;
        }

        public override void Refresh() {
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

                slotDisplay.SlotToDisplay = tile.WorkerSlot;

                slotDisplay.transform.position = Camera.main.WorldToScreenPoint(tile.transform.position);
                slotDisplay.Refresh();

                slotDisplay.gameObject.SetActive(true);
            }
        }

        #endregion

        private void OnDistributionPerformed(ICity city) {
            if(city == CityToDisplay) {
                Refresh();
            }
        }

        private WorkerSlotDisplay GetNextSlotDisplay(int currentIndex) {
            if(currentIndex >= InstantiatedDisplays.Count) {
                var newDisplay = SlotFactory.Create();

                newDisplay.transform.SetParent(transform, false);
                newDisplay.gameObject.SetActive(false);

                InstantiatedDisplays.Add(newDisplay);
            }

            return InstantiatedDisplays[currentIndex];
        }

        #endregion

    }

}
