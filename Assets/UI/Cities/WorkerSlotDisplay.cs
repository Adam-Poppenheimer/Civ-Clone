using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation;
using Assets.Simulation.WorkerSlots;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.ResourceGeneration;

namespace Assets.UI.Cities {

    public class WorkerSlotDisplay : MonoBehaviour, IPointerClickHandler {

        #region instance fields and properties

        public IWorkerSlot SlotToDisplay { get; set; }

        public ICity Owner { get; set; }

        [SerializeField] private Image SlotImage;

        [SerializeField] private ResourceSummaryDisplay YieldDisplay;

        private IDisposable OccupiedSubscription;
        private IDisposable UnoccupiedSubcription;
        private IDisposable LockedSubcription;
        private IDisposable UnlockedSubcription;





        private ICityUIConfig Config;

        private WorkerSlotSignals SlotSignals;

        private IResourceGenerationLogic ResourceGenerationLogic;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            ICityUIConfig config, WorkerSlotSignals slotSignals,
            IResourceGenerationLogic resourceGenerationLogic
        ){
            Config                  = config;
            SlotSignals             = slotSignals;
            ResourceGenerationLogic = resourceGenerationLogic;
        }

        #region Unity messages

        private void OnEnable() {
            if(SlotToDisplay == null) {
                return;
            }

            OccupiedSubscription  = SlotSignals.SlotBecameOccupied .Subscribe(CheckSlotForRefresh);
            UnoccupiedSubcription = SlotSignals.SlotBecameUnoccuped.Subscribe(CheckSlotForRefresh);
            LockedSubcription     = SlotSignals.SlotBecameLocked   .Subscribe(CheckSlotForRefresh);
            UnlockedSubcription   = SlotSignals.SlotBecameUnlocked .Subscribe(CheckSlotForRefresh);

            Refresh();
        }

        private void OnDisable() {
            OccupiedSubscription .Dispose();
            UnoccupiedSubcription.Dispose();
            LockedSubcription    .Dispose();
            UnlockedSubcription  .Dispose();
        }

        #endregion

        #region EventSystem handler implementations

        public void OnPointerClick(PointerEventData eventData) {
            if(SlotToDisplay != null) {
                SlotSignals.SlotClicked.OnNext(SlotToDisplay);
            }            
        }

        #endregion

        private void Refresh() {
            if(SlotToDisplay.IsOccupied) {
                SlotImage.material = SlotToDisplay.IsLocked ? Config.LockedSlotMaterial : Config.OccupiedSlotMaterial;
            }else {
                SlotImage.material = Config.UnoccupiedSlotMaterial;
            }

            if(YieldDisplay != null) {
                YieldDisplay.DisplaySummary(ResourceGenerationLogic.GetYieldOfSlotForCity(SlotToDisplay, Owner));
            }
        }

        private void CheckSlotForRefresh(IWorkerSlot slot) {
            if(slot == SlotToDisplay) {
                Refresh();
            }
        }

        #endregion

    }

}
