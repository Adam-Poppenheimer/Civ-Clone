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

        public IBuilding BuildingToDisplay { get; set; }

        [SerializeField] private Text NameField;
        [SerializeField] private Text MaintenanceField;
        [SerializeField] private ResourceSummaryDisplay StaticYieldDisplay;

        [SerializeField] private Transform SlotDisplayParent;

        private List<IWorkerSlotDisplay> InstantiatedSlotDisplays = new List<IWorkerSlotDisplay>();

        private WorkerSlotDisplayFactory SlotDisplayFactory;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(WorkerSlotDisplayFactory slotDisplayFactory) {
            SlotDisplayFactory = slotDisplayFactory;
        }

        #region from IBuildingDisplay

        public void Refresh() {
            foreach(var slotDisplay in InstantiatedSlotDisplays) {
                slotDisplay.gameObject.SetActive(false);
            }

            NameField       .text = BuildingToDisplay.Template.name;
            MaintenanceField.text = BuildingToDisplay.Template.Maintenance.ToString();

            StaticYieldDisplay.DisplaySummary(BuildingToDisplay.Template.StaticYield);

            int slotDisplayIndex = 0;
            foreach(var slot in BuildingToDisplay.Slots) {
                var slotDisplay = GetSlotDisplay(slotDisplayIndex++);
                slotDisplay.SlotToDisplay = slot;

                slotDisplay.gameObject.SetActive(true);
                slotDisplay.Refresh();
            }
        }

        #endregion

        private IWorkerSlotDisplay GetSlotDisplay(int index) {
            if(InstantiatedSlotDisplays.Count == index) {
                var newDisplay = SlotDisplayFactory.Create();

                newDisplay.gameObject.transform.SetParent(SlotDisplayParent, false);

                InstantiatedSlotDisplays.Add(newDisplay);
            }
            return InstantiatedSlotDisplays[index];
        }

        #endregion
        
    }

}
