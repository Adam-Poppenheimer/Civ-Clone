using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units {

    /// <summary>
    /// For Unit signals that require specific execution order.
    /// </summary>
    public class UnitResponder {

        #region instance fields and properties

        private IUnitPositionCanon UnitPositionCanon;

        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public UnitResponder(UnitSignals signals, IUnitPositionCanon unitPositionCanon,
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon
        ) {
            UnitPositionCanon   = unitPositionCanon;
            UnitPossessionCanon = unitPossessionCanon;

            signals.UnitBeingDestroyedSignal.Subscribe(OnUnitBeingDestroyed);
        }

        #endregion

        #region instance methods

        private void OnUnitBeingDestroyed(IUnit unit) {
            UnitPositionCanon  .ChangeOwnerOfPossession(unit, null);
            UnitPossessionCanon.ChangeOwnerOfPossession(unit, null);
        }

        #endregion

    }
}
