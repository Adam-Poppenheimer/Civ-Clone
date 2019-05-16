﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Zenject;
using TMPro;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;

namespace Assets.UI.Cities.Buildings {

    public class BuildingDisplay : MonoBehaviour, IBuildingDisplay, IPointerEnterHandler, IPointerExitHandler {

        #region instance fields and properties

        public IBuilding BuildingToDisplay { get; set; }

        public ICity Owner { get; set; }

        [SerializeField] private Text NameField;
        [SerializeField] private Image IconField;

        [SerializeField] private Transform SlotDisplayContainer;
        [SerializeField] private WorkerSlotDisplay SlotDisplayPrefab;

        [SerializeField] private Button DestroyButton;

        private List<WorkerSlotDisplay> InstantiatedSlotDisplays = new List<WorkerSlotDisplay>();




        private DiContainer        Container;
        private DescriptionTooltip Tooltip;
        private IBuildingFactory   BuildingFactory;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            DiContainer container, DescriptionTooltip tooltip, IBuildingFactory buildingFactory
        ){
            Container       = container;
            Tooltip         = tooltip;
            BuildingFactory = buildingFactory;
        }

        #region EventSystem implementations

        public void OnPointerEnter(PointerEventData eventData) {
            Tooltip.SetDescriptionFrom(BuildingToDisplay.Template);

            Tooltip.gameObject.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData) {
            //Tooltip.gameObject.SetActive(false);
        }

        #endregion

        #region from IBuildingDisplay

        public void Refresh(bool includeDestroyButton) {
            foreach(var slotDisplay in InstantiatedSlotDisplays) {
                Destroy(slotDisplay.gameObject);
            }
            InstantiatedSlotDisplays.Clear();

            NameField.text = BuildingToDisplay.name;

            if(BuildingToDisplay.Slots.Count > 0) {
                SlotDisplayContainer.gameObject.SetActive(true);

                foreach(var slot in BuildingToDisplay.Slots) {
                    var newSlotDisplay = Container.InstantiatePrefabForComponent<WorkerSlotDisplay>(SlotDisplayPrefab);

                    newSlotDisplay.Owner         = Owner;
                    newSlotDisplay.SlotToDisplay = slot;                    

                    newSlotDisplay.transform.SetParent(SlotDisplayContainer, false);
                    newSlotDisplay.transform.SetAsFirstSibling();
                    newSlotDisplay.gameObject.SetActive(true);

                    InstantiatedSlotDisplays.Add(newSlotDisplay);
                }
            }else {
                SlotDisplayContainer.gameObject.SetActive(false);
            }

            DestroyButton.gameObject.SetActive(includeDestroyButton);
        }

        #endregion

        public void BuildingDestructionRequested() {
            BuildingFactory.DestroyBuilding(BuildingToDisplay);
        }

        #endregion

    }

}
