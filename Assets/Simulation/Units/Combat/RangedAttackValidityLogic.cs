using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units.Combat {

    public class RangedAttackValidityLogic : IRangedAttackValidityLogic {

        #region instance fields and properties
        
        private ICommonAttackValidityLogic CommonAttackValidityLogic;
        private IUnitPositionCanon         UnitPositionCanon;
        private IHexGrid                   Grid;
        private IUnitVisibilityLogic      UnitLineOfSightLogic;

        #endregion

        #region constructors

        [Inject]
        public RangedAttackValidityLogic(
            ICommonAttackValidityLogic commonAttackValidityLogic, IUnitPositionCanon unitPositionCanon,
            IHexGrid grid, IUnitVisibilityLogic unitLineOfSightLogic
        ) {
            CommonAttackValidityLogic = commonAttackValidityLogic;
            UnitPositionCanon         = unitPositionCanon;
            Grid                      = grid;
            UnitLineOfSightLogic      = unitLineOfSightLogic;
        }

        #endregion

        #region instance methods

        #region from IRangedAttackValidityLogic

        public bool IsRangedAttackValid(IUnit attacker, IUnit defender) {
            if(!CommonAttackValidityLogic.DoesAttackMeetCommonConditions(attacker, defender)) {
                return false;
            }

            var attackerLocation = UnitPositionCanon.GetOwnerOfPossession(attacker);
            var defenderLocation = UnitPositionCanon.GetOwnerOfPossession(defender);

            if(Grid.GetDistance(attackerLocation, defenderLocation) > attacker.AttackRange) {
                return false;
            }

            if(attacker.CurrentMovement <= 0 || !attacker.IsReadyForRangedAttack) {
                return false;
            }

            var attackerVisibleCells = UnitLineOfSightLogic.GetCellsVisibleToUnit(attacker);

            if( !attacker.CombatSummary.IgnoresLineOfSight &&
                !attackerVisibleCells.Contains(defenderLocation)
            ){
                return false;
            }

            if(attacker.RangedAttackStrength <= 0) {
                return false;
            }

            return true;
        }

        #endregion

        #endregion
        
    }

}
