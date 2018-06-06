using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.HexMap {

    public class RoadTriangulator : IRoadTriangulator {

        #region instance fields and properties

        private IHexGridMeshBuilder MeshBuilder;

        #endregion

        #region constructors

        [Inject]
        public RoadTriangulator(IHexGridMeshBuilder meshBuilder) {
            MeshBuilder = meshBuilder;
        }

        #endregion

        #region instance methods

        #region from IRoadTriangulator

        public bool ShouldTriangulateRoads(CellTriangulationData data) {
            return false;
        }

        public void TriangulateRoads(CellTriangulationData data) {

        }

        #endregion

        #endregion

    }

}
