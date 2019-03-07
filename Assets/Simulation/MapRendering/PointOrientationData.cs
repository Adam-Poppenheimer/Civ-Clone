using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public class PointOrientationData {

        #region instance fields and properties

        public bool IsOnGrid;

        public HexDirection Sextant;

        public IHexCell Center;
        public IHexCell Left;
        public IHexCell Right;
        public IHexCell NextRight;

        public float CenterWeight;
        public float LeftWeight;
        public float RightWeight;
        public float NextRightWeight;

        public float RiverWeight;

        #endregion

    }

}
