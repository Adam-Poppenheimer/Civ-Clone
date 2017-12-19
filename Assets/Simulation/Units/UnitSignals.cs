using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation.GameMap;
using Assets.Simulation.Units.Abilities;

namespace Assets.Simulation.Units {

    public class UnitSignals {

        #region instance fields and properties

        public ISubject<IUnit> UnitClickedSignal { get; private set; }

        public ISubject<Tuple<IUnit, PointerEventData>> UnitBeginDragSignal { get; private set; }
        public ISubject<Tuple<IUnit, PointerEventData>> UnitDragSignal      { get; private set; }
        public ISubject<Tuple<IUnit, PointerEventData>> UnitEndDragSignal   { get; private set; }

        public ISubject<Tuple<IUnit, IMapTile>>               UnitLocationChangedSignal  { get; private set; }
        public ISubject<Tuple<IUnit, IUnitAbilityDefinition>> UnitActivatedAbilitySignal { get; private set; }

        #endregion

        #region constructors

        [Inject]
        public UnitSignals(
            [Inject(Id = "Unit Clicked Signal"   )] ISubject<IUnit> unitClickedSignal,
            [Inject(Id = "Unit Begin Drag Signal")] ISubject<Tuple<IUnit, PointerEventData>> unitBeginDragSignal,
            [Inject(Id = "Unit Drag Signal"      )] ISubject<Tuple<IUnit, PointerEventData>> unitDragSignal,
            [Inject(Id = "Unit End Drag Signal"  )] ISubject<Tuple<IUnit, PointerEventData>> unitEndDragSignal,
            [Inject(Id = "Unit Location Changed Signal")]  ISubject<Tuple<IUnit, IMapTile>> unitLocationChangedSignal,
            [Inject(Id = "Unit Activated Ability Signal")] ISubject<Tuple<IUnit, IUnitAbilityDefinition>> unitActivatedAbilitySignal
        ) {
            UnitClickedSignal          = unitClickedSignal;

            UnitBeginDragSignal        = unitBeginDragSignal;
            UnitDragSignal             = unitDragSignal;
            UnitEndDragSignal          = unitEndDragSignal;

            UnitLocationChangedSignal  = unitLocationChangedSignal;
            UnitActivatedAbilitySignal = unitActivatedAbilitySignal;
        }

        #endregion

    }

}
