using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

namespace Assets.Simulation.Units {

    public class UnitSignals {

        #region instance fields and properties

        public ISubject<IUnit> UnitClickedSignal { get; private set; }

        #endregion

        #region constructors

        [Inject]
        public UnitSignals(
            [Inject(Id = "Unit Clicked Signal")] ISubject<IUnit> unitClickedSignal
        ) {
            UnitClickedSignal = unitClickedSignal;
        }

        #endregion

    }

}
