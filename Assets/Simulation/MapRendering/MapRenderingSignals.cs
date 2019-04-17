using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public class MapRenderingSignals {

        #region instance fields and properties

        public ISubject<Unit> FarmlandsTriangulated { get; private set; }

        #endregion

        #region constructors

        public MapRenderingSignals() {
            FarmlandsTriangulated = new Subject<Unit>();
        }

        #endregion

    }

}
