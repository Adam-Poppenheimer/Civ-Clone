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
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Territory;
using Assets.Simulation.HexMap;

namespace Assets.UI.Cities.Territory {

    public class CityTerritoryDisplay : CityDisplayBase {

        #region instance fields and properties

        [SerializeField] private WorkerSlotDisplay SlotDisplayPrefab;
        [SerializeField] private RectTransform SlotDisplayContainer;

        private List<WorkerSlotDisplay> InstantiatedDisplays = new List<WorkerSlotDisplay>();

        private List<IDisposable> SignalSubscriptions = new List<IDisposable>();




        private IPossessionRelationship<ICity, IHexCell> PossessionCanon;
        private CitySignals                              CitySignals;
        private HexCellSignals                           HexCellSignals;
        private DiContainer                              Container;
        private IBorderExpansionLogic                    BorderExpansionLogic;
        private IPossessionRelationship<ICity, IHexCell> CellPossessionCanon;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IPossessionRelationship<ICity, IHexCell> possessionCanon, CitySignals citySignals,
            HexCellSignals hexCellSignals, DiContainer container, IBorderExpansionLogic borderExpansionLogic,
            IPossessionRelationship<ICity, IHexCell> cellPossessionCanon
        ){
            PossessionCanon      = possessionCanon;
            CitySignals          = citySignals;
            HexCellSignals       = hexCellSignals;
            Container            = container;
            BorderExpansionLogic = borderExpansionLogic;
            CellPossessionCanon  = cellPossessionCanon;
        }

        #region from CityDisplayBase

        protected override void DoOnEnable() {
            SignalSubscriptions.Add(CitySignals   .DistributionPerformed.Subscribe(OnDistributionPerformed));
            SignalSubscriptions.Add(HexCellSignals.Clicked              .Subscribe(OnCellClicked));
        }

        protected override void DoOnDisable() {
            SignalSubscriptions.ForEach(subscription => subscription.Dispose());
            SignalSubscriptions.Clear();
        }

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

        private void OnCellClicked(UniRx.Tuple<IHexCell, PointerEventData> data) {
            if(DisplayType != CityDisplayType.MapEditor) {
                return;
            }

            var cell = data.Item1;

            if(CellPossessionCanon.GetOwnerOfPossession(cell) == ObjectToDisplay) {
                if(CellPossessionCanon.CanChangeOwnerOfPossession(cell, null)) {
                    CellPossessionCanon.ChangeOwnerOfPossession(cell, null);
                }

            }else if(BorderExpansionLogic.IsCellAvailable(ObjectToDisplay, cell)) {
                CellPossessionCanon.ChangeOwnerOfPossession(cell, ObjectToDisplay);
            }
        }

        #endregion

    }

}
