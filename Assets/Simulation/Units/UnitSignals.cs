using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation.Core;
using Assets.Simulation.HexMap;
using Assets.Simulation.Units.Abilities;
using Assets.Simulation.Units.Combat;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Units {

    public class UnitSignals {

        #region instance fields and properties

        public ISubject<IUnit> Clicked { get; private set; }

        public ISubject<IUnit> PointerEntered { get; private set; }
        public ISubject<IUnit> PointerExited  { get; private set; }

        public ISubject<UniRx.Tuple<IUnit, PointerEventData>> BeginDrag { get; private set; }
        public ISubject<UniRx.Tuple<IUnit, PointerEventData>> Drag      { get; private set; }
        public ISubject<UniRx.Tuple<IUnit, PointerEventData>> EndDrag   { get; private set; }

        public ISubject<UniRx.Tuple<IUnit, IAbilityDefinition>> ActivatedAbility { get; private set; }

        public ISubject<UniRx.Tuple<IUnit, IHexCell>> LeftLocation    { get; private set; }
        public ISubject<UniRx.Tuple<IUnit, IHexCell>> EnteredLocation { get; private set; }

        public ISubject<IUnit> StoppedMoving       { get; private set; }
        public ISubject<IUnit> SetUpForBombardment { get; private set; }
        public ISubject<IUnit> BecameIdle          { get; private set; }

        public ISubject<IUnit> BeingDestroyed { get; private set; }

        public ISubject<UnitCombatResults> MeleeCombatWithUnit  { get; private set; }
        public ISubject<UnitCombatResults> RangedCombatWithUnit { get; private set; }

        public ISubject<IUnit> ExperienceChanged { get; private set; }
        public ISubject<IUnit> LevelChanged      { get; private set; }

        public ISubject<UniRx.Tuple<IUnit, int>> GainedExperience { get; private set; }

        public ISubject<IUnit> GainedPromotion { get; private set; }

        public ISubject<UniRx.Tuple<IUnit, ICivilization>> GainedNewOwner { get; private set; }

        public ISubject<IUnit> HitpointsChanged { get; private set; }

        public ISubject<IUnit> NewUnitCreated { get; private set; }

        #endregion

        #region constructors

        [Inject]
        public UnitSignals(){
            Clicked        = new Subject<IUnit>();
            PointerEntered = new Subject<IUnit>();
            PointerExited  = new Subject<IUnit>();

            BeginDrag        = new Subject<UniRx.Tuple<IUnit, PointerEventData>>();
            Drag             = new Subject<UniRx.Tuple<IUnit, PointerEventData>>();
            EndDrag          = new Subject<UniRx.Tuple<IUnit, PointerEventData>>();

            ActivatedAbility = new Subject<UniRx.Tuple<IUnit, IAbilityDefinition>>();

            LeftLocation    = new Subject<UniRx.Tuple<IUnit, IHexCell>>();
            EnteredLocation = new Subject<UniRx.Tuple<IUnit, IHexCell>>();

            StoppedMoving       = new Subject<IUnit>();
            SetUpForBombardment = new Subject<IUnit>();
            BecameIdle          = new Subject<IUnit>();

            BeingDestroyed   = new Subject<IUnit>();

            MeleeCombatWithUnit  = new Subject<UnitCombatResults>();
            RangedCombatWithUnit = new Subject<UnitCombatResults>();

            ExperienceChanged = new Subject<IUnit>();
            LevelChanged      = new Subject<IUnit>();

            GainedExperience = new Subject<UniRx.Tuple<IUnit, int>>();

            GainedPromotion = new Subject<IUnit>();

            GainedNewOwner = new Subject<UniRx.Tuple<IUnit, ICivilization>>();

            HitpointsChanged = new Subject<IUnit>();

            NewUnitCreated = new Subject<IUnit>();
        }

        #endregion

    }

}
