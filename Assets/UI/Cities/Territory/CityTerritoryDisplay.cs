using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;
using UniRx;

using Assets.Simulation;
using Assets.Simulation.Cities;
using Assets.Simulation.HexMap;

namespace Assets.UI.Cities.Territory {

    public class CityTerritoryDisplay : CityDisplayBase {

        #region instance fields and properties

        [SerializeField] private WorkerSlotDisplay SlotDisplayPrefab;
        [SerializeField] private RectTransform SlotDisplayContainer;

        private List<WorkerSlotDisplay> InstantiatedDisplays = new List<WorkerSlotDisplay>();




        private IPossessionRelationship<ICity, IHexCell> PossessionCanon;
        
        private DiContainer Container;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IPossessionRelationship<ICity, IHexCell> possessionCanon, CitySignals signals,
            DiContainer container
        ){
            PossessionCanon = possessionCanon;
            Container       = container;
            
            signals.DistributionPerformedSignal.Listen(OnDistributionPerformed);
        }

        #region from CityDisplayBase

        public override void Refresh() {
            if(ObjectToDisplay == null) {
                return;
            }

            foreach(var display in InstantiatedDisplays) {
                Destroy(display.gameObject);
            }
            InstantiatedDisplays.Clear();

            foreach(var tile in PossessionCanon.GetPossessionsOfOwner(ObjectToDisplay)) {
                if(tile.SuppressSlot) {
                    continue;
                }

                var newSlot = Container.InstantiatePrefabForComponent<WorkerSlotDisplay>(SlotDisplayPrefab);

                newSlot.Owner         = ObjectToDisplay;
                newSlot.SlotToDisplay = tile.WorkerSlot;

                newSlot.transform.SetParent(SlotDisplayContainer, false);
                newSlot.gameObject.transform.position = Camera.main.WorldToScreenPoint(tile.AbsolutePosition);
                newSlot.gameObject.SetActive(true);

                InstantiatedDisplays.Add(newSlot);
            }
        }

        #endregion

        private void OnDistributionPerformed(ICity city) {
            if(city == ObjectToDisplay) {
                Refresh();
            }
        }

        #endregion

    }

}
