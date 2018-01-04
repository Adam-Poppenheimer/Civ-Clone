using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;
using Assets.Simulation.Units.Abilities;

namespace Assets.Simulation.Units {

    public class UnitSignals {

        #region instance fields and properties

        public ISubject<IUnit> UnitClickedSignal { get; private set; }

        public ISubject<Tuple<IUnit, PointerEventData>> UnitBeginDragSignal { get; private set; }
        public ISubject<Tuple<IUnit, PointerEventData>> UnitDragSignal      { get; private set; }
        public ISubject<Tuple<IUnit, PointerEventData>> UnitEndDragSignal   { get; private set; }

        public ISubject<Tuple<IUnit, IHexCell>>               UnitLocationChangedSignal  { get; private set; }
        public ISubject<Tuple<IUnit, IUnitAbilityDefinition>> UnitActivatedAbilitySignal { get; private set; }

        #endregion

        #region constructors

        [Inject]
        public UnitSignals() {
            UnitClickedSignal          = new Subject<IUnit>();

            UnitBeginDragSignal        = new Subject<Tuple<IUnit, PointerEventData>>();
            UnitDragSignal             = new Subject<Tuple<IUnit, PointerEventData>>();
            UnitEndDragSignal          = new Subject<Tuple<IUnit, PointerEventData>>();

            UnitLocationChangedSignal  = new Subject<Tuple<IUnit, IHexCell>>();
            UnitActivatedAbilitySignal = new Subject<Tuple<IUnit, IUnitAbilityDefinition>>();
        }

        #endregion

    }

}
