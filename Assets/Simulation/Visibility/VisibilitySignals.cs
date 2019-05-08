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

        public ISubject<Unit> CellVisibilityModeChanged     { get; private set; }
        public ISubject<Unit> ResourceVisibilityModeChanged { get; private set; }

        public ISubject<Unit> CellExplorationModeChanged    { get; private set; }

        public ISubject<UniRx.Tuple<IHexCell, ICivilization>> CellBecameExploredByCiv  { get; private set; }
        public ISubject<UniRx.Tuple<IHexCell, ICivilization>> CellBecameVisibleToCiv   { get; private set; }
        public ISubject<UniRx.Tuple<IHexCell, ICivilization>> CellBecameInvisibleToCiv { get; private set; }

        #endregion

        #region constructors

        public VisibilitySignals() {
            CellVisibilityModeChanged     = new Subject<Unit>();
            ResourceVisibilityModeChanged = new Subject<Unit>();

            CellExplorationModeChanged    = new Subject<Unit>();

            CellBecameExploredByCiv  = new Subject<UniRx.Tuple<IHexCell, ICivilization>>();
            CellBecameVisibleToCiv   = new Subject<UniRx.Tuple<IHexCell, ICivilization>>();
            CellBecameInvisibleToCiv = new Subject<UniRx.Tuple<IHexCell, ICivilization>>();
        }

        #endregion

    }
}
