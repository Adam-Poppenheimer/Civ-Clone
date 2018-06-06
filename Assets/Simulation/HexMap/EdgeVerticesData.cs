using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.HexMap {

    public class EdgeVerticesData {

        #region instance fields and properties

        public EdgeVertices Edge { get; set; }

        public Color Weights { get; set; }

        public float Index { get; set; }

        public bool PerturbY { get; set; }

        public bool UMin { get; set; }
        public bool UMax { get; set; }

        public bool V { get; set; }

        public Color Color { get; set; }

        #endregion

    }

}
