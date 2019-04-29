using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation.Cities;
using Assets.Simulation.HexMap;
using Assets.Util;

namespace Assets.Simulation.MapRendering {

    public class CellEventEmitter : MonoBehaviour, IPointerDownHandler,
        IPointerUpHandler, IPointerClickHandler, IPointerEnterHandler,
        IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {

        #region instance fields and properties

        private Collider[] Colliders;

        private IHexCell LastCellEntered;

        private IHexCell CellBeingDragged;

        private bool ShouldEmitEnterExitMessages;




        private HexCellSignals                           CellSignals;
        private CitySignals                              CitySignals;
        private IPossessionRelationship<IHexCell, ICity> CityLocationCanon;
        private IPointOrientationLogic             PointOrientationLogic;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            HexCellSignals cellSignals, CitySignals citySignals,
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon,
            IPointOrientationLogic pointOrientationLogic
        ){
            CellSignals           = cellSignals;
            CitySignals           = citySignals;
            CityLocationCanon     = cityLocationCanon;
            PointOrientationLogic = pointOrientationLogic;
        }

        #region Unity messages

        private void Start() {
            Colliders = GetComponentsInChildren<Collider>();
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

                if(DidRaycastHitChunk(hit)) {
                    IHexCell cell = PointOrientationLogic.GetCellAtPoint(hit.point);

                    if(cell != null) {
                        CellSignals.PointerDown.OnNext(new Tuple<IHexCell, PointerEventData>(cell, eventData));
                    }
                }
            }
        }

        public void OnPointerUp(PointerEventData eventData) {
            var pointerRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(pointerRay, out hit, float.MaxValue)) {

                if(DidRaycastHitChunk(hit)) {
                    IHexCell cell = PointOrientationLogic.GetCellAtPoint(hit.point);

                    if(cell != null) {
                        CellSignals.PointerUp.OnNext(new Tuple<IHexCell, PointerEventData>(cell, eventData));
                    }
                }
            }
        }

        public void OnPointerClick(PointerEventData eventData) {
            var pointerRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(pointerRay, out hit, float.MaxValue)) {

                if(DidRaycastHitChunk(hit)) {
                    IHexCell cell = PointOrientationLogic.GetCellAtPoint(hit.point);

                    if(cell == null) {
                        return;
                    }

                    var cityAtLocation = GetCityAtLocation(cell);
                    if(cityAtLocation != null) {
                        CitySignals.PointerClicked.OnNext(cityAtLocation);
                    }else {
                        CellSignals.Clicked.OnNext(
                            new Tuple<IHexCell, PointerEventData>(cell, eventData)
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
                CellSignals.BeginDrag.OnNext(new HexCellDragData() {
                    CellBeingDragged = CellBeingDragged,
                    EventData        = eventData
                });
            }
        }

        public void OnDrag(PointerEventData eventData) {
            if(CellBeingDragged != null) {
                CellSignals.Drag.OnNext(new HexCellDragData() {
                    CellBeingDragged = CellBeingDragged,
                    EventData        = eventData
                });
            }
        }

        public void OnEndDrag(PointerEventData eventData) {
            if(CellBeingDragged != null) {
                CellSignals.EndDrag.OnNext(new HexCellDragData() {
                    CellBeingDragged = CellBeingDragged,
                    EventData        = eventData
                });
            }

            CellBeingDragged = null;
        }

        #endregion

        private bool DidRaycastHitChunk(RaycastHit hit) {
            return Colliders.Any(collider => collider == hit.collider);
        }

        private IHexCell GetCellUnderPosition(Vector3 position) {
            var pointerRay = Camera.main.ScreenPointToRay(position);

            RaycastHit hit;
            if(Physics.Raycast(pointerRay, out hit, float.MaxValue)) {
                if(DidRaycastHitChunk(hit)) {
                    return PointOrientationLogic.GetCellAtPoint(hit.point);
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
            CellSignals.PointerEnter.OnNext(LastCellEntered);
        }

        private void EmitExitMessage(IHexCell cell) {
            CellSignals.PointerExit.OnNext(cell);
        }

        private void EmitEnterMessage(ICity city) {
            CitySignals.PointerEntered.OnNext(city);
        }

        private void EmitExitMessage(ICity city) {
            CitySignals.PointerExited.OnNext(city);
        }

        private ICity GetCityAtLocation(IHexCell location) {
            return CityLocationCanon.GetPossessionsOfOwner(location).FirstOrDefault();
        }

        #endregion

    }

}
