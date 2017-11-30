using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Zenject;

using Assets.Simulation;

namespace Assets.UI.Cities {

    public class WorkerSlotDisplay : MonoBehaviour, IPointerClickHandler, IWorkerSlotDisplay {

        #region internal types

        public class Factory : Factory<WorkerSlotDisplay> { }

        #endregion

        #region instance fields and properties

        public IWorkerSlot SlotToDisplay { get; set; }

        [SerializeField] private Image SlotImage;

        private SlotDisplayClickedSignal ClickedSignal;

        private ICityUIConfig Config;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(SlotDisplayClickedSignal clickedSignal, ICityUIConfig config) {
            ClickedSignal = clickedSignal;
            Config = config;
        }

        #region EventSystem handler implementations

        public void OnPointerClick(PointerEventData eventData) {
            if(SlotToDisplay != null) {
                ClickedSignal.Fire(this);
            }            
        }

        #endregion

        public void Refresh() {
            if(SlotToDisplay == null) {
                return;
            }

            if(SlotToDisplay.IsOccupied) {
                SlotImage.material = SlotToDisplay.IsLocked ? Config.LockedSlotMaterial : Config.OccupiedSlotMaterial;
            }else {
                SlotImage.material = Config.UnoccupiedSlotMaterial;
            }
        }        

        #endregion

    }

}
