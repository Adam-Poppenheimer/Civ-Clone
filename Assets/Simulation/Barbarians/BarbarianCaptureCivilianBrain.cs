using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.AI;
using Assets.Simulation.HexMap;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Combat;

namespace Assets.Simulation.Barbarians {

    public class BarbarianCaptureCivilianBrain : IBarbarianGoalBrain {

        #region instance fields and properties

        private IHexGrid             Grid;
        private IUnitPositionCanon   UnitPositionCanon;
        private IHexPathfinder       HexPathfinder;
        private IBarbarianConfig     BarbarianConfig;
        private IBarbarianBrainTools BrainTools;
        private DiContainer          Container;

        #endregion

        #region constructors

        [Inject]
        public BarbarianCaptureCivilianBrain(
            IHexGrid grid, IUnitPositionCanon unitPositionCanon, IHexPathfinder hexPathfinder,
            IBarbarianConfig barbarianConfig, IBarbarianBrainTools brainTools, DiContainer container
        ) {
            Grid              = grid;
            UnitPositionCanon = unitPositionCanon;
            HexPathfinder     = hexPathfinder;
            BarbarianConfig   = barbarianConfig;
            BrainTools        = brainTools;
            Container         = container;
        }

        #endregion

        #region instance methods

        #region from IBarbarianGoalBrain

        public float GetUtilityForUnit(IUnit unit, InfluenceMaps maps) {
            var unitPosition = UnitPositionCanon.GetOwnerOfPossession(unit);

            var reachableCells = HexPathfinder.GetAllCellsReachableIn(
                unitPosition, unit.CurrentMovement,
                (current, next) => UnitPositionCanon.GetTraversalCostForUnit(unit, current, next, true),
                Grid.Cells
            );

            if(reachableCells.Keys.Any(BrainTools.GetCaptureCivilianFilter(unit))) {
                return BarbarianConfig.CaptureCivilianUtility;
            }else {
                return 0f;
            }
        }

        public List<IUnitCommand> GetCommandsForUnit(IUnit unit, InfluenceMaps maps) {
            var unitPosition = UnitPositionCanon.GetOwnerOfPossession(unit);

            var reachableCells = HexPathfinder.GetAllCellsReachableIn(
                unitPosition, unit.CurrentMovement,
                (current, next) => UnitPositionCanon.GetTraversalCostForUnit(unit, current, next, true),
                Grid.Cells
            );

            var cellToAttack = reachableCells.Keys.FirstOrDefault(BrainTools.GetCaptureCivilianFilter(unit));

            var retval = new List<IUnitCommand>();

            if(cellToAttack != null) {
                var attackCommand = Container.Instantiate<AttackUnitCommand>();

                attackCommand.Attacker         = unit;
                attackCommand.LocationToAttack = cellToAttack;
                attackCommand.CombatType       = CombatType.Melee;

                retval.Add(attackCommand);
            }

            return retval;
        }

        #endregion

        #endregion
        
    }

}
