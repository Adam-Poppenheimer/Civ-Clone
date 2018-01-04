﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;

namespace Assets.Simulation.HexMap {

    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class HexMesh : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {

        #region instance fields and properties

        private Mesh ManagedMesh;
        public MeshCollider Collider { get; private set; }

        private HexCellSignals CellSignals;
        private IHexGrid Grid;

        private List<Vector3> Vertices;
        private List<int>     Triangles;
        private List<Color>   Colors;

        private IHexCell LastCellEntered;

        private bool ShouldEmitEnterExitMessages;        

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(HexCellSignals cellSignals, IHexGrid grid) {
            CellSignals = cellSignals;
            Grid = grid;
        }


        #region Unity message methods

        private void Awake() {
            GetComponent<MeshFilter>().mesh = ManagedMesh = new Mesh();
            ManagedMesh.name = "Hex Mesh";
            
            Collider = GetComponent<MeshCollider>();

            Vertices  = new List<Vector3>();
            Triangles = new List<int>();
            Colors    = new List<Color>();
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

        public void Triangulate(HexCell[] cells) {
            ManagedMesh.Clear();
            Vertices   .Clear();
            Colors     .Clear();
            Triangles  .Clear();

            for(int i = 0; i < cells.Length; ++i) {
                Triangulate(cells[i]);
            }

            ManagedMesh.vertices  = Vertices .ToArray();
            ManagedMesh.triangles = Triangles.ToArray();
            ManagedMesh.colors    = Colors   .ToArray();
            ManagedMesh.RecalculateNormals();

            Collider.sharedMesh = ManagedMesh;
        }

        private void Triangulate(HexCell cell) {
            Vector3 center = cell.transform.localPosition;

            for(int i = 0; i < 6; ++i) {
                AddTriangle(
                    center,
                    center + HexMetrics.Corners[i],
                    center + HexMetrics.Corners[i + 1]
                );

                AddTriangleColor(cell.Color);
            }
            
        }

        private void AddTriangle(Vector3 vertexOne, Vector3 vertexTwo, Vector3 vertexThree) {
            int vertexIndex = Vertices.Count;

            Vertices.Add(vertexOne);
            Vertices.Add(vertexTwo);
            Vertices.Add(vertexThree);

            Triangles.Add(vertexIndex);
            Triangles.Add(vertexIndex + 1);
            Triangles.Add(vertexIndex + 2);
        }

        private void AddTriangleColor(Color color) {
            Colors.Add(color);
            Colors.Add(color);
            Colors.Add(color);
        }

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
