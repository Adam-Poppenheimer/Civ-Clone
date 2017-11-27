using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation.Cities.Buildings;

namespace Assets.UI.Cities.Buildings {

    public class BuildingDisplay : MonoBehaviour, IBuildingDisplay {

        #region instance fields and properties

        [SerializeField] private Text NameField;
        [SerializeField] private Text MaintenanceField;
        [SerializeField] private ResourceSummaryDisplay StaticYieldDisplay;

        [SerializeField] private GameObject SlotDisplayPrefab;
        [SerializeField] private Transform SlotDisplayParent;

        private List<IWorkerSlotDisplay> InstantiatedSlotDisplays = new List<IWorkerSlotDisplay>();

        #endregion

        #region instance methods

        #region from IBuildingDisplay

        public void DisplayBuilding(IBuilding building, ICityUIConfig config) {
            foreach(var slotDisplay in InstantiatedSlotDisplays) {
                slotDisplay.gameObject.SetActive(false);
            }

            NameField       .text = building.Template.name;
            MaintenanceField.text = building.Template.Maintenance.ToString();

            StaticYieldDisplay.DisplaySummary(building.Template.StaticYield);

            int slotDisplayIndex = 0;
            foreach(var slot in building.Slots) {
                var slotDisplay = GetSlotDisplay(slotDisplayIndex++);

                slotDisplay.gameObject.SetActive(true);
                slotDisplay.DisplayOccupationStatus(slot.IsOccupied, config);
            }
        }

        #endregion

        private IWorkerSlotDisplay GetSlotDisplay(int index) {
            if(InstantiatedSlotDisplays.Count == index) {
                var newSlotPrefab = Instantiate(SlotDisplayPrefab);

                var newSlot = newSlotPrefab.GetComponent<IWorkerSlotDisplay>();
                newSlot.transform.SetParent(SlotDisplayParent, false);

                InstantiatedSlotDisplays.Add(newSlot);
            }
            return InstantiatedSlotDisplays[index];
        }

        #endregion
        
    }

}
