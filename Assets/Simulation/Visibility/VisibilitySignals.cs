using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UniRx;

using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Visibility {

    public class VisibilitySignals {

        #region instance fields and properties

        public ISubject<Unit> CellVisibilityModeChangedSignal     { get; private set; }
        public ISubject<Unit> ResourceVisibilityModeChangedSignal { get; private set; }

        public ISubject<Unit> CellExplorationModeChangedSignal    { get; private set; }

        public ISubject<Tuple<IHexCell, ICivilization>> CellBecameExploredByCivSignal   { get; private set; }
        public ISubject<Tuple<IHexCell, ICivilization>> CellBecameVisibleToCivSignal    { get; private set; }
        public ISubject<Tuple<IHexCell, ICivilization>> CellBecameInvisibleToCiv  { get; private set; }

        #endregion

        #region constructors

        public VisibilitySignals() {
            CellVisibilityModeChangedSignal     = new Subject<Unit>();
            ResourceVisibilityModeChangedSignal = new Subject<Unit>();

            CellExplorationModeChangedSignal    = new Subject<Unit>();

            CellBecameExploredByCivSignal  = new Subject<Tuple<IHexCell, ICivilization>>();
            CellBecameVisibleToCivSignal   = new Subject<Tuple<IHexCell, ICivilization>>();
            CellBecameInvisibleToCiv = new Subject<Tuple<IHexCell, ICivilization>>();
        }

        #endregion

    }
}
