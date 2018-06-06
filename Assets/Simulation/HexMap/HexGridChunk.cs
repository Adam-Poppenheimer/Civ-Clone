using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.HexMap {

    public class HexGridChunk : MonoBehaviour {

        #region instance fields and properties

        private HexCell[] Cells;

        private IHexGridMeshBuilder  MeshBuilder;
        private IHexFeatureManager   FeatureManager;
        private IHexCellTriangulator Triangulator;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IHexGridMeshBuilder meshBuilder, IHexFeatureManager featureManager,
            IHexCellTriangulator triangulator
        ){
            MeshBuilder    = meshBuilder;
            FeatureManager = featureManager;
            Triangulator   = triangulator;
        }

        #region Unity messages

        private void Awake() {
            Cells = new HexCell[HexMetrics.ChunkSizeX * HexMetrics.ChunkSizeZ];
        }

        private void LateUpdate() {
            TriangulateAllCells();
            enabled = false;
        }

        #endregion

        public void AddCell(int index, HexCell cell) {
            Cells[index] = cell;
            cell.transform.SetParent(transform, false);
        }

        public void Refresh() {
            enabled = true;
        }

        public void TriangulateAllCells() {
            MeshBuilder.ClearMeshes();
            FeatureManager.Clear();

            for(int i = 0; i < Cells.Length; ++i) {
                Triangulator.TriangulateCell(Cells[i]);
            }

            MeshBuilder.ApplyMeshes();
            FeatureManager.Apply();
        }

        #endregion

    }

}
