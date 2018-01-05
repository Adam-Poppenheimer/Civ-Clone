using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

namespace Assets.Simulation.HexMap {

    public class HexGridChunk : MonoBehaviour {

        #region instance fields and properties

        private HexCell[] Cells;

        private HexMesh HexMesh;

        #endregion

        #region instance methods

        #region Unity messages

        private void Awake() {
            HexMesh = GetComponent<HexMesh>();

            Cells = new HexCell[HexMetrics.ChunkSizeX * HexMetrics.ChunkSizeZ];
        }

        private void LateUpdate() {
            HexMesh.Triangulate(Cells);
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

        #endregion

    }

}
