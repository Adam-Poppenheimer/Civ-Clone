using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;
using Assets.Simulation.Units.Abilities;
using Assets.Simulation.Units.Combat;

namespace Assets.Simulation.Units {

    public class UnitSignals {

        #region instance fields and properties

        public ISubject<IUnit> ClickedSignal { get; private set; }

        public ISubject<IUnit> PointerEnteredSignal { get; private set; }
        public ISubject<IUnit> PointerExitedSignal  { get; private set; }

        public ISubject<Tuple<IUnit, PointerEventData>> BeginDragSignal { get; private set; }
        public ISubject<Tuple<IUnit, PointerEventData>> DragSignal      { get; private set; }
        public ISubject<Tuple<IUnit, PointerEventData>> EndDragSignal   { get; private set; }

        public ISubject<Tuple<IUnit, IHexCell>>               UnitLocationChangedSignal  { get; private set; }
        public ISubject<Tuple<IUnit, IUnitAbilityDefinition>> UnitActivatedAbilitySignal { get; private set; }

        public ISubject<IUnit> UnitBeingDestroyedSignal { get; private set; }

        public ISubject<CombatResultData> CombatEventOccurredSignal { get; private set; }

        #endregion

        #region constructors

        [Inject]
        public UnitSignals() {
            ClickedSignal    = new Subject<IUnit>();
            PointerEnteredSignal = new Subject<IUnit>();
            PointerExitedSignal  = new Subject<IUnit>();

            BeginDragSignal        = new Subject<Tuple<IUnit, PointerEventData>>();
            DragSignal             = new Subject<Tuple<IUnit, PointerEventData>>();
            EndDragSignal          = new Subject<Tuple<IUnit, PointerEventData>>();

            UnitLocationChangedSignal  = new Subject<Tuple<IUnit, IHexCell>>();
            UnitActivatedAbilitySignal = new Subject<Tuple<IUnit, IUnitAbilityDefinition>>();

            UnitBeingDestroyedSignal   = new Subject<IUnit>();

            CombatEventOccurredSignal = new Subject<CombatResultData>();
        }

        #endregion

    }

}
