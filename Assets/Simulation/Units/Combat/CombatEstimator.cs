using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

namespace Assets.Simulation.Units.Combat {

    public class CombatEstimator : ICombatEstimator {

        #region instance fields and properties

        private IUnitPositionCanon UnitPositionCanon;
        private ICombatInfoLogic   CombatInfoLogic;
        private ICombatCalculator  CombatCalculator;

        #endregion

        #region constructors

        [Inject]
        public CombatEstimator(
            IUnitPositionCanon unitPositionCanon, ICombatInfoLogic combatInfoLogic,
            ICombatCalculator combatCalculator
        ) {
            UnitPositionCanon = unitPositionCanon;
            CombatInfoLogic   = combatInfoLogic;
            CombatCalculator  = combatCalculator;
        }

        #endregion

        #region instance methods

        #region ICombatEstimator

        public UnitCombatResults EstimateMeleeAttackResults(IUnit attacker, IUnit defender) {
            if(attacker == null) {
                throw new ArgumentNullException("attacker");
            }
            if(defender == null) {
                throw new ArgumentNullException("defender");
            }

            var defenderLocation = UnitPositionCanon.GetOwnerOfPossession(defender);

            var combatInfo = CombatInfoLogic.GetAttackInfo(attacker, defender, defenderLocation, CombatType.Melee);

            Tuple<int, int> results = CombatCalculator.CalculateCombat(attacker, defender, combatInfo);

            return new UnitCombatResults(attacker, defender, results.Item1, results.Item2, combatInfo);
        }

        public UnitCombatResults EstimateRangedAttackResults(IUnit attacker, IUnit defender) {
            if(attacker == null) {
                throw new ArgumentNullException("attacker");
            }
            if(defender == null) {
                throw new ArgumentNullException("defender");
            }

            var defenderLocation = UnitPositionCanon.GetOwnerOfPossession(defender);

            var combatInfo = CombatInfoLogic.GetAttackInfo(attacker, defender, defenderLocation, CombatType.Ranged);

            Tuple<int, int> results = CombatCalculator.CalculateCombat(attacker, defender, combatInfo);

            return new UnitCombatResults(attacker, defender, results.Item1, results.Item2, combatInfo);
        }

        #endregion

        #endregion

    }

}
