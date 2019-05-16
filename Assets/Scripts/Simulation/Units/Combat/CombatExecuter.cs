using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units.Combat {

    public class CombatExecuter : ICombatExecuter {

        #region instance fields and properties

        private IMeleeAttackValidityLogic   MeleeAttackValidityLogic;
        private IRangedAttackValidityLogic  RangedAttackValidityLogic;
        private IUnitPositionCanon          UnitPositionCanon;
        private IHexGrid                    Grid;
        private ICombatInfoLogic            CombatInfoLogic;
        private UnitSignals                 UnitSignals;
        private IHexPathfinder              HexPathfinder;
        private ICommonCombatExecutionLogic CommonCombatExecutionLogic;

        #endregion

        #region constructors

        [Inject]
        public CombatExecuter(
            IMeleeAttackValidityLogic meleeAttackValidityLogic, IRangedAttackValidityLogic rangedAttackValidityLogic,
            IUnitPositionCanon unitPositionCanon, IHexGrid grid, ICombatInfoLogic combatModifierLogic, UnitSignals unitSignals,
            IHexPathfinder hexPathfinder, ICommonCombatExecutionLogic commonCombatExecutionLogic
        ){
            MeleeAttackValidityLogic   = meleeAttackValidityLogic;
            RangedAttackValidityLogic  = rangedAttackValidityLogic;
            UnitPositionCanon          = unitPositionCanon;
            Grid                       = grid;
            CombatInfoLogic            = combatModifierLogic;
            UnitSignals                = unitSignals;
            HexPathfinder              = hexPathfinder;
            CommonCombatExecutionLogic = commonCombatExecutionLogic;
        }

        #endregion

        #region instance methods

        #region from ICombatExecuter

        public bool CanPerformMeleeAttack(IUnit attacker, IUnit defender) {
            return MeleeAttackValidityLogic.IsMeleeAttackValid(attacker, defender);
        }

        public bool CanPerformRangedAttack(IUnit attacker, IUnit defender) {
            return RangedAttackValidityLogic.IsRangedAttackValid(attacker, defender);
        }

        public void PerformMeleeAttack(IUnit attacker, IUnit defender, Action successAction, Action failAction) {
            if(!CanPerformMeleeAttack(attacker, defender)) {
                throw new InvalidOperationException("CanPerformMeleeCombat must return true");
            }

            var attackerLocation = UnitPositionCanon.GetOwnerOfPossession(attacker);
            var defenderLocation = UnitPositionCanon.GetOwnerOfPossession(defender);

            var pathToDefender = HexPathfinder.GetShortestPathBetween(
                attackerLocation, defenderLocation, attacker.CurrentMovement,
                (current, next) => UnitPositionCanon.GetTraversalCostForUnit(attacker, current, next, true),
                Grid.Cells
            );

            attacker.CurrentPath = pathToDefender.GetRange(0, pathToDefender.Count - 1);

            attacker.PerformMovement(
                false, PerformMeleeAttack_GetPostMovementCallback(
                    attacker, defender, defenderLocation, successAction, failAction
                )
            );
        }

        private Action PerformMeleeAttack_GetPostMovementCallback(
            IUnit attacker, IUnit defender, IHexCell defenderLocation, Action successAction, Action failAction
        ) {
            return delegate() {
                if(!CanPerformMeleeAttack(attacker, defender)) {
                    failAction();
                }else {
                    int attackerStartingHealth = attacker.CurrentHitpoints;
                    int defenderStartingHealth = defender.CurrentHitpoints;

                    var combatInfo = CombatInfoLogic.GetAttackInfo(attacker, defender, defenderLocation, CombatType.Melee);

                    CommonCombatExecutionLogic.PerformCommonCombatTasks(attacker, defender, combatInfo);

                    UnitSignals.MeleeCombatWithUnit.OnNext(
                        new UnitCombatResults(
                            attacker, defender,
                            attackerStartingHealth - attacker.CurrentHitpoints,
                            defenderStartingHealth - defender.CurrentHitpoints,
                            combatInfo
                        )
                    );

                    successAction();
                }
            };
        }

        public void PerformRangedAttack(IUnit attacker, IUnit defender) {
            if(!CanPerformRangedAttack(attacker, defender)) {
                throw new InvalidOperationException("CanPerformRangedCombat must return true");
            }

            int attackerStartingHealth = attacker.CurrentHitpoints;
            int defenderStartingHealth = defender.CurrentHitpoints;

            var defenderLocation = UnitPositionCanon.GetOwnerOfPossession(defender);

            var combatInfo = CombatInfoLogic.GetAttackInfo(attacker, defender, defenderLocation, CombatType.Ranged);

            CommonCombatExecutionLogic.PerformCommonCombatTasks(attacker, defender, combatInfo);

            UnitSignals.RangedCombatWithUnit.OnNext(
                new UnitCombatResults(
                    attacker, defender,
                    attackerStartingHealth - attacker.CurrentHitpoints,
                    defenderStartingHealth - defender.CurrentHitpoints,
                    combatInfo
                )
            );
        }

        #endregion

        #endregion

    }

}
