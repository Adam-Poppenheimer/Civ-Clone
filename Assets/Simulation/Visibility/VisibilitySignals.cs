using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UniRx;

namespace Assets.Simulation.Visibility {

    public class VisibilitySignals {

        #region instance fields and properties

        public ISubject<Unit> CellVisibilityModeChangedSignal     { get; private set; }
        public ISubject<Unit> ResourceVisibilityModeChangedSignal { get; private set; }

        #endregion

        #region constructors

        public VisibilitySignals() {
            CellVisibilityModeChangedSignal     = new Subject<Unit>();
            ResourceVisibilityModeChangedSignal = new Subject<Unit>();
        }

        #endregion

    }
}
