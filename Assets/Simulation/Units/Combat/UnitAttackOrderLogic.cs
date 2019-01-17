using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units.Combat {

    public class UnitAttackOrderLogic : IUnitAttackOrderLogic {

        #region instance fields and properties

        private IUnitPositionCanon UnitPositionCanon;

        #endregion

        #region constructors

        [Inject]
        public UnitAttackOrderLogic(IUnitPositionCanon unitPositionCanon) {
            UnitPositionCanon = unitPositionCanon;
        }

        #endregion

        #region instance methods

        #region from IUnitAttackOrderLogic

        public IUnit GetNextAttackTargetOnCell(IHexCell cell) {
            var unitsAt = UnitPositionCanon.GetPossessionsOfOwner(cell);

            if(!unitsAt.Any()) {
                return null;
            }

            IUnit candidate;

            candidate = unitsAt.FirstOrDefault(unit => unit.Type == UnitType.City);

            if(candidate != null) {
                return candidate;
            }

            candidate = unitsAt.FirstOrDefault(unit => unit.Type.IsWaterMilitary());

            if(candidate !=  null) {
                return candidate;
            }

            candidate = unitsAt.FirstOrDefault(unit => unit.Type.IsLandMilitary());

            if(candidate != null) {
                return candidate;
            }

            return unitsAt.FirstOrDefault();
        }

        #endregion

        #endregion
        
    }

}
