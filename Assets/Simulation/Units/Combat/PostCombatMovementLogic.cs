﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Simulation.Units.Combat {

    public class PostCombatMovementLogic : IPostCombatMovementLogic {

        #region instance fields and properties

        private IUnitPositionCanon UnitPositionCanon;

        #endregion

        #region constructors

        [Inject]
        public PostCombatMovementLogic(IUnitPositionCanon unitPositionCanon) {
            UnitPositionCanon = unitPositionCanon;
        }

        #endregion

        #region instance methods

        #region from IPostCombatMovementLogic

        public void HandleAttackerMovementAfterCombat(
            IUnit attacker, IUnit defender, CombatInfo combatInfo
        ){
            if(!attacker.CombatSummary.CanMoveAfterAttacking) {
                attacker.CurrentMovement = 0;

            }else if(combatInfo.CombatType == CombatType.Melee) {
                var attackerLocation = UnitPositionCanon.GetOwnerOfPossession(attacker);
                var defenderLocation = UnitPositionCanon.GetOwnerOfPossession(defender);

                var costToMove = UnitPositionCanon.GetTraversalCostForUnit(
                    attacker, attackerLocation, defenderLocation, true
                );

                attacker.CurrentMovement = Math.Max(0, attacker.CurrentMovement - costToMove);

            }else if(combatInfo.CombatType == CombatType.Ranged) {
                attacker.CurrentMovement--;
            }
        }

        #endregion

        #endregion

    }

}
