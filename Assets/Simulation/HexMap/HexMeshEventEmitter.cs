using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation.Cities;

namespace Assets.Simulation.HexMap {

    public class HexMeshEventEmitter : MonoBehaviour, IPointerDownHandler,
        IPointerUpHandler, IPointerClickHandler, IPointerEnterHandler,
        IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {

        #region instance fields and properties

        public MeshCollider Collider { get; private set; }

        private IHexCell LastCellEntered;

        private IHexCell CellBeingDragged;

        private bool ShouldEmitEnterExitMessages;




        private HexCellSignals                           CellSignals;
        private IHexGrid                                 Grid;
        private CitySignals                              CitySignals;
        private IPossessionRelationship<IHexCell, ICity> CityLocationCanon;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            HexCellSignals cellSignals, IHexGrid grid, CitySignals citySignals,
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon
        ){
            CellSignals       = cellSignals;
            Grid              = grid;
            CitySignals       = citySignals;
            CityLocationCanon = cityLocationCanon;
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

        public void OnPointerDown(PointerEventData eventData) {
            var pointerRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(pointerRay, out hit, float.MaxValue)) {

                if(hit.collider == Collider) {
                    if(Grid.HasCellAtLocation(hit.point)) {
                        var clickedCell = Grid.GetCellAtLocation(hit.point);

                        CellSignals.PointerDownSignal.OnNext(new Tuple<IHexCell, PointerEventData>(clickedCell, eventData));
                    }
                }
            }
        }

        public void OnPointerUp(PointerEventData eventData) {
            var pointerRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(pointerRay, out hit, float.MaxValue)) {

                if(hit.collider == Collider) {
                    if(Grid.HasCellAtLocation(hit.point)) {
                        var unclickedCell = Grid.GetCellAtLocation(hit.point);

                        CellSignals.PointerUpSignal.OnNext(new Tuple<IHexCell, PointerEventData>(unclickedCell, eventData));
                    }
                }
            }
        }

        public void OnPointerClick(PointerEventData eventData) {
            var pointerRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(pointerRay, out hit, float.MaxValue)) {

                if(hit.collider == Collider) {
                    if(!Grid.HasCellAtLocation(hit.point)) {
                        return;
                    }

                    var clickedCell = Grid.GetCellAtLocation(hit.point);

                    var cityAtLocation = GetCityAtLocation(clickedCell);
                    if(cityAtLocation != null) {
                        CitySignals.PointerClickedSignal.OnNext(cityAtLocation);
                    }else {
                        CellSignals.ClickedSignal.OnNext(
                            new Tuple<IHexCell, PointerEventData>(clickedCell, eventData)
                        );
                    }
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData) {
            ShouldEmitEnterExitMessages = true;
        }

        public void OnPointerExit(PointerEventData eventData) {
            ShouldEmitEnterExitMessages = false;
            if(LastCellEntered != null) {
                var cityAtLocation = GetCityAtLocation(LastCellEntered);

                if(cityAtLocation != null) {
                    EmitExitMessage(cityAtLocation);
                }else {
                    EmitExitMessage(LastCellEntered);
                }

                LastCellEntered = null;
            }
        }

        public void OnBeginDrag(PointerEventData eventData) {
            CellBeingDragged = GetCellUnderPosition(Input.mousePosition);

            if(CellBeingDragged != null) {
                CellSignals.BeginDragSignal.OnNext(new HexCellDragData() {
                    CellBeingDragged = CellBeingDragged,
                    EventData        = eventData
                });
            }
        }

        public void OnDrag(PointerEventData eventData) {
            if(CellBeingDragged != null) {
                CellSignals.DragSignal.OnNext(new HexCellDragData() {
                    CellBeingDragged = CellBeingDragged,
                    EventData        = eventData
                });
            }
        }

        public void OnEndDrag(PointerEventData eventData) {
            if(CellBeingDragged != null) {
                CellSignals.EndDragSignal.OnNext(new HexCellDragData() {
                    CellBeingDragged = CellBeingDragged,
                    EventData        = eventData
                });
            }

            CellBeingDragged = null;
        }

        #endregion

        private IHexCell GetCellUnderPosition(Vector3 position) {
            var pointerRay = Camera.main.ScreenPointToRay(position);

            RaycastHit hit;
            if(Physics.Raycast(pointerRay, out hit, float.MaxValue)) {
                if(hit.collider == Collider) {
                    return Grid.HasCellAtLocation(hit.point) ? Grid.GetCellAtLocation(hit.point) : null;
                }
            }

            return null;
        }

        private void TryEmitEnterExitMessages() {
            if( ShouldEmitEnterExitMessages &&
                (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
            ){
                var cellUnderMouse = GetCellUnderPosition(Input.mousePosition);
                if(cellUnderMouse != null && cellUnderMouse != LastCellEntered) {
                    if(LastCellEntered != null) {
                        var cityAtLocation = GetCityAtLocation(LastCellEntered);

                        if(cityAtLocation != null) {
                            EmitExitMessage(cityAtLocation);
                        }else {
                            EmitExitMessage(LastCellEntered);
                        }
                    }

                    LastCellEntered = cellUnderMouse;

                    if(LastCellEntered != null) {
                        var cityAtLocation = GetCityAtLocation(LastCellEntered);

                        if(cityAtLocation != null) {
                            EmitEnterMessage(cityAtLocation);
                        }else {
                            EmitEnterMessage(LastCellEntered);
                        }
                    }
                }
            }
        }

        private void EmitEnterMessage(IHexCell cell) {
            CellSignals.PointerEnterSignal.OnNext(LastCellEntered);
        }

        private void EmitExitMessage(IHexCell cell) {
            CellSignals.PointerExitSignal.OnNext(cell);
        }

        private void EmitEnterMessage(ICity city) {
            CitySignals.PointerEnteredSignal.OnNext(city);
        }

        private void EmitExitMessage(ICity city) {
            CitySignals.PointerExitedSignal.OnNext(city);
        }

        private ICity GetCityAtLocation(IHexCell location) {
            return CityLocationCanon.GetPossessionsOfOwner(location).FirstOrDefault();
        }

        #endregion

    }

}
