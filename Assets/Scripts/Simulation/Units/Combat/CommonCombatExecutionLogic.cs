using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

namespace Assets.Simulation.Units.Combat {

    public class CommonCombatExecutionLogic : ICommonCombatExecutionLogic {

        #region instance fields and properties

        private ICombatCalculator          CombatCalculator;
        private List<IPostCombatResponder> PostCombatResponders;

        #endregion

        #region constructors

        [Inject]
        public CommonCombatExecutionLogic(
            ICombatCalculator combatCalculator, List<IPostCombatResponder> postCombatResponders
        ) {
            CombatCalculator     = combatCalculator;
            PostCombatResponders = postCombatResponders;
        }

        #endregion

        #region instance methods

        #region from ICommonCombatExecutionLogic

        public void PerformCommonCombatTasks(IUnit attacker, IUnit defender, CombatInfo combatInfo) {
            Tuple<int, int> results = CombatCalculator.CalculateCombat(attacker, defender, combatInfo);

            attacker.CurrentHitpoints -= results.Item1;
            defender.CurrentHitpoints -= results.Item2;

            foreach(var responder in PostCombatResponders) {
                responder.RespondToCombat(attacker, defender, combatInfo);
            }

            attacker.CanAttack = attacker.CombatSummary.CanAttackAfterAttacking;
        }

        #endregion

        #endregion
        
    }

}
