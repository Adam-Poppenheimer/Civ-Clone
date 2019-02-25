using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public class RiverSection {

        public IHexCell AdjacentCellOne;
        public IHexCell AdjacentCellTwo;

        public HexDirection DirectionFromOne;

        public RiverFlow FlowFromOne;

        public Vector3 Start;
        public Vector3 End;

        public bool HasPreviousEndpoint;
        public bool HasNextEndpoint;

        public bool PreviousOnInternalCurve;
        public bool NextOnInternalCurve;

    }

}
