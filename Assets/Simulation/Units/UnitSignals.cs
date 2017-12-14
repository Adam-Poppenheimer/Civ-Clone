using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine.EventSystems;

using Zenject;
using UniRx;

namespace Assets.Simulation.Units {

    public class UnitSignals {

        #region instance fields and properties

        public ISubject<IUnit> UnitClickedSignal { get; private set; }

        public ISubject<Tuple<IUnit, PointerEventData>> UnitBeginDragSignal { get; private set; }
        public ISubject<Tuple<IUnit, PointerEventData>> UnitDragSignal      { get; private set; }
        public ISubject<Tuple<IUnit, PointerEventData>> UnitEndDragSignal   { get; private set; }

        #endregion

        #region constructors

        [Inject]
        public UnitSignals(
            [Inject(Id = "Unit Clicked Signal"   )] ISubject<IUnit> unitClickedSignal,
            [Inject(Id = "Unit Begin Drag Signal")] ISubject<Tuple<IUnit, PointerEventData>> unitBeginDragSignal,
            [Inject(Id = "Unit Drag Signal"      )] ISubject<Tuple<IUnit, PointerEventData>> unitDragSignal,
            [Inject(Id = "Unit End Drag Signal"  )] ISubject<Tuple<IUnit, PointerEventData>> unitEndDragSignal
        ) {
            UnitClickedSignal = unitClickedSignal;

            UnitBeginDragSignal = unitBeginDragSignal;
            UnitDragSignal      = unitDragSignal;
            UnitEndDragSignal   = unitEndDragSignal;
        }

        #endregion

    }

}
