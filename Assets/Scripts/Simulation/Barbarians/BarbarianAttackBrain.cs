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

    public class BarbarianAttackBrain : IBarbarianGoalBrain {

        #region instance fields and properties

        private IUnitVisibilityLogic       UnitVisibilityLogic;
        private IUnitPositionCanon         UnitPositionCanon;
        private IBarbarianUtilityLogic     UtilityLogic;
        private IBarbarianBrainFilterLogic FilterLogic;
        private IHexGrid                   Grid;
        private DiContainer                Container;

        #endregion

        #region constructors

        [Inject]
        public BarbarianAttackBrain(
            IUnitVisibilityLogic unitVisibilityLogic, IUnitPositionCanon unitPositionCanon,
            IBarbarianUtilityLogic utilityLogic, IBarbarianBrainFilterLogic filterLogic,
            IHexGrid grid, DiContainer container
        ) {
            UnitVisibilityLogic = unitVisibilityLogic;
            UnitPositionCanon   = unitPositionCanon;
            UtilityLogic        = utilityLogic;
            FilterLogic         = filterLogic;
            Grid                = grid;
            Container           = container;
        }

        #endregion

        #region instance methods

        #region from IBarbarianGoalBrain

        public float GetUtilityForUnit(IUnit unit, InfluenceMaps maps) {
            if(!unit.CanAttack) {
                return 0f;
            }

            IEnumerable<IHexCell> attackCandidates = null;

            if(unit.RangedAttackStrength > 0) {
                var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

                attackCandidates = Grid.GetCellsInRadius(unitLocation, unit.AttackRange)
                                       .Where(FilterLogic.GetRangedAttackFilter(unit));
            }else {
                attackCandidates = UnitVisibilityLogic.GetCellsVisibleToUnit(unit)
                                                      .Where(FilterLogic.GetMeleeAttackFilter(unit));
            }

            if(attackCandidates.Any()) {
                return attackCandidates.Max(UtilityLogic.GetAttackUtilityFunction(unit, maps));
            }else {
                return 0f;
            }
        }

        public List<IUnitCommand> GetCommandsForUnit(IUnit unit, InfluenceMaps maps) {
            IEnumerable<IHexCell> attackCandidates = null;

            CombatType combatType;

            if(unit.RangedAttackStrength > 0) {
                var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

                attackCandidates = Grid.GetCellsInRadius(unitLocation, unit.AttackRange)
                                       .Where(FilterLogic.GetRangedAttackFilter(unit));

                combatType = CombatType.Ranged;

            }else {
                attackCandidates = UnitVisibilityLogic.GetCellsVisibleToUnit(unit)
                                                      .Where(FilterLogic.GetMeleeAttackFilter(unit));

                combatType = CombatType.Melee;
            }

            var retval = new List<IUnitCommand>();

            if(attackCandidates.Any()) {
                IHexCell bestCandidate = attackCandidates.MaxElement(UtilityLogic.GetAttackUtilityFunction(unit, maps));

                if(bestCandidate != null) {
                    var attackCommand = Container.Instantiate<AttackUnitCommand>();

                    attackCommand.Attacker         = unit;
                    attackCommand.LocationToAttack = bestCandidate;
                    attackCommand.CombatType       = combatType;

                    retval.Add(attackCommand);
                }
            }

            return retval;
        }

        #endregion

        #endregion

    }

}
