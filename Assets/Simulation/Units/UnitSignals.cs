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

        public ISubject<Tuple<IUnit, IAbilityDefinition>> ActivatedAbilitySignal { get; private set; }

        public ISubject<Tuple<IUnit, IHexCell>> LeftLocationSignal    { get; private set; }
        public ISubject<Tuple<IUnit, IHexCell>> EnteredLocationSignal { get; private set; }

        public ISubject<IUnit> UnitBeingDestroyedSignal { get; private set; }

        public ISubject<UnitUnitCombatData> MeleeCombatWithUnitSignal { get; private set; }
        public ISubject<UnitCityCombatData> MeleeCombatWithCitySignal { get; private set; }

        public ISubject<UnitUnitCombatData> RangedCombatWithUnitSignal { get; private set; }
        public ISubject<UnitCityCombatData> RangedCombatWithCitySignal { get; private set; }

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

            ActivatedAbilitySignal = new Subject<Tuple<IUnit, IAbilityDefinition>>();

            LeftLocationSignal    = new Subject<Tuple<IUnit, IHexCell>>();
            EnteredLocationSignal = new Subject<Tuple<IUnit, IHexCell>>();

            UnitBeingDestroyedSignal   = new Subject<IUnit>();

            MeleeCombatWithUnitSignal = new Subject<UnitUnitCombatData>();
            MeleeCombatWithCitySignal = new Subject<UnitCityCombatData>();

            RangedCombatWithUnitSignal = new Subject<UnitUnitCombatData>();
            RangedCombatWithCitySignal = new Subject<UnitCityCombatData>();
        }

        #endregion

    }

}
