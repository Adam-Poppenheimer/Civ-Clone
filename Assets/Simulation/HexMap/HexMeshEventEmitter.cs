using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;

namespace Assets.Simulation.HexMap {

    public class HexMeshEventEmitter : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {

        #region instance fields and properties

        public MeshCollider Collider { get; private set; }

        private IHexCell LastCellEntered;

        private bool ShouldEmitEnterExitMessages;

        private HexCellSignals CellSignals;
        private IHexGrid Grid;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(HexCellSignals cellSignals, IHexGrid grid) {
            CellSignals = cellSignals;
            Grid = grid;
        }

        #region Unity messages

        private void Start() {
            Collider = GetComponent<MeshCollider>();
        }

        private void Update() {
            TryEmitEnterExitMessages();
        }

        #endregion

        #region EventSystem handler implementations

        public void OnPointerClick(PointerEventData eventData) {
            var pointerRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(pointerRay, out hit, float.MaxValue)) {

                if(hit.collider == Collider) {
                    var coordinates = HexCoordinates.FromPosition(hit.point);
                    var clickedCell = Grid.GetCellAtCoordinates(coordinates);
                    CellSignals.ClickedSignal.Fire(clickedCell, Input.mousePosition);
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData) {
            ShouldEmitEnterExitMessages = true;
        }

        public void OnPointerExit(PointerEventData eventData) {
            ShouldEmitEnterExitMessages = false;
            if(LastCellEntered != null) {
                EmitExitMessage(LastCellEntered);
                LastCellEntered = null;
            }
        }

        #endregion

        private IHexCell GetCellUnderPosition(Vector3 position) {
            var pointerRay = Camera.main.ScreenPointToRay(position);

            RaycastHit hit;
            if(Physics.Raycast(pointerRay, out hit, float.MaxValue)) {
                if(hit.collider == Collider) {
                    var coordinates = HexCoordinates.FromPosition(hit.point);
                    return Grid.GetCellAtCoordinates(coordinates);
                }
            }

            return null;
        }

        private void TryEmitEnterExitMessages() {
            if( ShouldEmitEnterExitMessages &&
                (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
            ){
                var cellUnderMouse = GetCellUnderPosition(Input.mousePosition);
                if(cellUnderMouse != LastCellEntered) {
                    if(LastCellEntered != null) {
                        EmitExitMessage(LastCellEntered);
                    }

                    LastCellEntered = cellUnderMouse;

                    if(LastCellEntered != null) {
                        EmitEnterMessage(LastCellEntered);
                    }
                }
            }
        }

        private void EmitEnterMessage(IHexCell cell) {
            CellSignals.PointerEnterSignal.Fire(LastCellEntered);
        }

        private void EmitExitMessage(IHexCell cell) {
            CellSignals.PointerExitSignal.Fire(cell);
        }

        #endregion

    }

}
