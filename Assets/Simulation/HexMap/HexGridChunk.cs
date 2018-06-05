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

        [SerializeField] private HexFeatureManager FeatureManager;

        private IHexGrid                  Grid;
        private IRiverTriangulator        RiverTriangulator;
        private IHexGridMeshBuilder       MeshBuilder;
        private ICultureTriangulator      CultureTriangulator;
        private IBasicTerrainTriangulator BasicTerrainTriangulator;
        private IWaterTriangulator        WaterTriangulator;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IHexGrid grid, IRiverTriangulator riverTriangulator,
            IHexGridMeshBuilder meshBuilder, ICultureTriangulator cultureTriangulator,
            IBasicTerrainTriangulator basicTerrainTriangulator,
            IWaterTriangulator waterTriangulator
        ){
            Grid                     = grid;
            RiverTriangulator        = riverTriangulator;
            MeshBuilder              = meshBuilder;
            CultureTriangulator      = cultureTriangulator;
            BasicTerrainTriangulator = basicTerrainTriangulator;
            WaterTriangulator        = waterTriangulator;
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
                TriangulateCell(Cells[i]);
            }

            MeshBuilder.ApplyMeshes();
            FeatureManager.Apply();
        }

        private void TriangulateCell(IHexCell cell) {
            FeatureManager.FlagLocationForFeatures(cell.transform.localPosition, cell);

            for(HexDirection direction = HexDirection.NE; direction <= HexDirection.NW; ++direction) {
                TriangulateInDirection(direction, cell);
            }
        }

        private void TriangulateInDirection(HexDirection direction, IHexCell cell) {
            var previousNeighbor = Grid.GetNeighbor(cell, direction.Previous());
            var neighbor         = Grid.GetNeighbor(cell, direction);

            var data = MeshBuilder.GetTriangulationData(
                cell, previousNeighbor, neighbor, direction
            );

            BasicTerrainTriangulator.TriangulateTerrainCenter(data);

            foreach(var featureLocation in data.CenterFeatureLocations) {
                FeatureManager.FlagLocationForFeatures(featureLocation, cell);
            }

            if(RiverTriangulator.ShouldTriangulateRiverConnection(data)) {
                RiverTriangulator.TriangulateRiverConnection(data);
            }
            

            if(BasicTerrainTriangulator.ShouldTriangulateTerrainConnection(data)) {
                BasicTerrainTriangulator.TriangulateTerrainConnection(data);
            }

            if(WaterTriangulator.ShouldTriangulateWater(data)) {
                WaterTriangulator.TriangulateWater(data);
            }

            if(CultureTriangulator.ShouldTriangulateCulture(data)) {
                CultureTriangulator.TriangulateCulture(data);               
            }
        }

        #endregion

    }

}
