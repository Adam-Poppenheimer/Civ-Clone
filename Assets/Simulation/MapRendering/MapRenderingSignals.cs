using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

namespace Assets.Simulation.MapRendering {

    public class MapRenderingSignals {

        #region instance fields and properties

        public ISubject<Unit> FarmlandsRefreshed { get; private set; }

        #endregion

        #region constructors

        public MapRenderingSignals() {
            FarmlandsRefreshed = new Subject<Unit>();
        }

        #endregion

    }

}
