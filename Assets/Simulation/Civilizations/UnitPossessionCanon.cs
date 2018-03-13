using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.Units;
using Assets.Simulation.SpecialtyResources;
using Assets.Simulation.Cities;

namespace Assets.Simulation.Civilizations {

    public class UnitPossessionCanon : PossessionRelationship<ICivilization, IUnit> {

        #region instance fields and properties

        private IResourceAssignmentCanon ResourceAssignmentCanon;

        #endregion

        #region constructors

        [Inject]
        public UnitPossessionCanon(IResourceAssignmentCanon resourceAssignmentCanon,
            CivilizationSignals civSignals
        ) {
            ResourceAssignmentCanon = resourceAssignmentCanon;

            civSignals.CivilizationBeingDestroyedSignal.Subscribe(OnCivilizationBeingDestroyed);
        }

        #endregion

        #region instance methods

        #region from PossessionRelationship<ICivilization, IUnit>

        protected override void DoOnPossessionEstablished(IUnit unit, ICivilization newOwner) {
            foreach(var resource in unit.RequiredResources) {
                ResourceAssignmentCanon.ReserveCopyOfResourceForCiv(resource, newOwner);
            }
        }

        protected override void DoOnPossessionBroken(IUnit unit, ICivilization oldOwner) {
            foreach(var resource in unit.RequiredResources) {
                ResourceAssignmentCanon.UnreserveCopyOfResourceForCiv(resource, oldOwner);
            }
        }

        #endregion

        private void OnCivilizationBeingDestroyed(ICivilization civ) {
            foreach(var unit in GetPossessionsOfOwner(civ)) {
                if(!(unit.Type == UnitType.City)) {
                    GameObject.Destroy(unit.gameObject);
                }
            }
        }

        #endregion

    }

}
