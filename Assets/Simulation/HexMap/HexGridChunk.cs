﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.Profiling;

using Zenject;

namespace Assets.Simulation.HexMap {

    public class HexGridChunk : MonoBehaviour {

        #region instance fields and properties

        private HexCell[] Cells;

        private IHexGridMeshBuilder  MeshBuilder;
        private IHexFeatureManager   FeatureManager;
        private IHexCellTriangulator Triangulator;
        private IHexMapRenderConfig  RenderConfig;
        private IRiverCanon          RiverCanon;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IHexGridMeshBuilder meshBuilder, IHexFeatureManager featureManager,
            IHexCellTriangulator triangulator, IHexMapRenderConfig renderConfig,
            IRiverCanon riverCanon
        ){
            MeshBuilder    = meshBuilder;
            FeatureManager = featureManager;
            Triangulator   = triangulator;
            RenderConfig   = renderConfig;
            RiverCanon     = riverCanon;
        }

        #region Unity messages

        private void Awake() {
            Cells = new HexCell[RenderConfig.ChunkSizeX * RenderConfig.ChunkSizeZ];
        }

        private void LateUpdate() {
            TriangulateAllCells();

            StartCoroutine(PopulateAllFeatures());

            foreach(var cell in Cells) {
                RiverCanon.ValidateRivers(cell);
            }

            enabled = false;
        }

        #endregion

        public void AddCell(int index, HexCell cell) {
            Cells[index] = cell;
        }

        public void Refresh() {
            enabled = true;
        }

        public void TriangulateAllCells() {
            MeshBuilder.ClearMeshes();            

            for(int i = 0; i < Cells.Length; ++i) {
                Triangulator.TriangulateCell(Cells[i]);
            }

            MeshBuilder.ApplyMeshes();
        }

        private IEnumerator PopulateAllFeatures() {
            yield return new WaitForEndOfFrame();

            FeatureManager.Clear();

            for(int i = 0; i < Cells.Length; ++i) {
                FeatureManager.AddFeatureLocationsForCell(Cells[i]);
            }

            FeatureManager.Apply();
        }

        #endregion

    }

}
