using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units.Combat {

    public class MeleeAttackValidityLogic : IMeleeAttackValidityLogic {

        #region instance fields and properties

        private IUnitPositionCanon         UnitPositionCanon;
        private IHexGrid                   Grid;
        private IHexPathfinder             HexPathfinder;
        private IUnitLineOfSightLogic      LineOfSightLogic;
        private ICommonAttackValidityLogic CommonAttackValidityLogic;

        #endregion

        #region constructors

        [Inject]
        public MeleeAttackValidityLogic(
            IUnitPositionCanon unitPositionCanon, IHexGrid grid, IHexPathfinder hexPathfinder,
            IUnitLineOfSightLogic lineOfSightLogic, ICommonAttackValidityLogic commandAttackValidityLogic
        ) {
            UnitPositionCanon         = unitPositionCanon;
            Grid                      = grid;
            HexPathfinder             = hexPathfinder;
            LineOfSightLogic          = lineOfSightLogic;
            CommonAttackValidityLogic = commandAttackValidityLogic;
        }

        #endregion

        #region instance methods

        #region from IMeleeAttackValidityLogic

        public bool IsMeleeAttackValid(IUnit attacker, IUnit defender) {
            if(!CommonAttackValidityLogic.DoesAttackMeetCommonConditions(attacker, defender)) {
                return false;
            }

            var attackerLocation = UnitPositionCanon.GetOwnerOfPossession(attacker);
            var defenderLocation = UnitPositionCanon.GetOwnerOfPossession(defender);

            var shortestPath = HexPathfinder.GetShortestPathBetween(
                attackerLocation, defenderLocation, attacker.CurrentMovement,
                (current, next) => UnitPositionCanon.GetTraversalCostForUnit(attacker, current, next, true),
                Grid.Cells
            );

            if(shortestPath == null) {
                return false;
            }

            if(!LineOfSightLogic.GetCellsVisibleToUnit(attacker).Contains(defenderLocation)) {
                return false;
            }

            if(attacker.CombatStrength <= 0) {
                return false;
            }

            return true;
        }

        #endregion

        #endregion
        
    }

}
