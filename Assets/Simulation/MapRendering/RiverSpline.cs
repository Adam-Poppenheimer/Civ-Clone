using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;
using Assets.Util;

namespace Assets.Simulation.MapRendering {

    public class RiverSpline {

        #region instance fields and properties

        public BezierSpline CenterSpline;

        public List<IHexCell> WesternCells;
        public List<IHexCell> EasternCells;

        public List<HexDirection> Directions;

        public List<RiverFlow> Flows;

        #endregion

    }

}
