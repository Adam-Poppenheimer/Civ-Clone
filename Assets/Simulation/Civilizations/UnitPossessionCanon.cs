﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.Units;

namespace Assets.Simulation.Civilizations {

    public class UnitPossessionCanon : PossessionRelationship<ICivilization, IUnit> {

        #region instance fields and properties

        private IResourceLockingCanon ResourceLockingCanon;
        private CivilizationSignals   CivSignals;
        private UnitSignals           UnitSignals;

        #endregion

        #region constructors

        [Inject]
        public UnitPossessionCanon(
            IResourceLockingCanon resourceLockingCanon, CivilizationSignals civSignals,
            UnitSignals unitSignals
        ) {
            ResourceLockingCanon = resourceLockingCanon;
            CivSignals           = civSignals;
            UnitSignals          = unitSignals;

            civSignals.CivBeingDestroyed.Subscribe(OnCivilizationBeingDestroyed);
        }

        #endregion

        #region instance methods

        #region from PossessionRelationship<ICivilization, IUnit>

        protected override void DoOnPossessionEstablished(IUnit unit, ICivilization newOwner) {
            foreach(var resource in unit.RequiredResources) {
                ResourceLockingCanon.LockCopyOfResourceForCiv(resource, newOwner);
            }
            
            CivSignals.CivGainedUnit.OnNext(new Tuple<ICivilization, IUnit>(newOwner, unit));
            UnitSignals.UnitGainedNewOwnerSignal.OnNext(new Tuple<IUnit, ICivilization>(unit, newOwner));
        }

        protected override void DoOnPossessionBeingBroken(IUnit possession, ICivilization oldOwner) {
            CivSignals.CivLosingUnit.OnNext(new Tuple<ICivilization, IUnit>(oldOwner, possession));
        }

        protected override void DoOnPossessionBroken(IUnit unit, ICivilization oldOwner) {
            foreach(var resource in unit.RequiredResources) {
                ResourceLockingCanon.UnlockCopyOfResourceForCiv(resource, oldOwner);
            }

            CivSignals.CivLostUnit.OnNext(new Tuple<ICivilization, IUnit>(oldOwner, unit));
            UnitSignals.UnitGainedNewOwnerSignal.OnNext(new Tuple<IUnit, ICivilization>(unit, null));
        }

        #endregion

        private void OnCivilizationBeingDestroyed(ICivilization civ) {
            foreach(var unit in GetPossessionsOfOwner(civ)) {
                if(!(unit.Type == UnitType.City)) {
                    unit.Destroy();
                }
            }
        }

        #endregion

    }

}
