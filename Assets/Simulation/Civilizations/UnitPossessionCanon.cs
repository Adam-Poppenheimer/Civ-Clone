using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.Units;

namespace Assets.Simulation.Civilizations {

    public class UnitPossessionCanon : PossessionRelationship<ICivilization, IUnit> {

        #region constructors

        [Inject]
        public UnitPossessionCanon(UnitSignals signals) {
            signals.UnitBeingDestroyedSignal.Subscribe(OnUnitBeingDestroyed);
        }

        #endregion

        #region instance methods

        private void OnUnitBeingDestroyed(IUnit unit){
            ChangeOwnerOfPossession(unit, null);
        }

        #endregion

    }

}
