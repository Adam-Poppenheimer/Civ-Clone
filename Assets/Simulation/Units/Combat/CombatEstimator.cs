using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units.Combat {

    public class CombatEstimator : ICombatEstimator {

        #region instance fields and properties

        private ICombatInfoLogic  CombatInfoLogic;
        private ICombatCalculator CombatCalculator;

        #endregion

        #region constructors

        [Inject]
        public CombatEstimator(
            ICombatInfoLogic combatInfoLogic, ICombatCalculator combatCalculator
        ) {
            CombatInfoLogic   = combatInfoLogic;
            CombatCalculator  = combatCalculator;
        }

        #endregion

        #region instance methods

        #region ICombatEstimator

        public UnitCombatResults EstimateMeleeAttackResults(IUnit attacker, IUnit defender, IHexCell location) {
            if(attacker == null) {
                throw new ArgumentNullException("attacker");
            }
            if(defender == null) {
                throw new ArgumentNullException("defender");
            }
            if(location == null) {
                throw new ArgumentNullException("location");
            }

            var combatInfo = CombatInfoLogic.GetAttackInfo(attacker, defender, location, CombatType.Melee);

            UniRx.Tuple<int, int> results = CombatCalculator.CalculateCombat(attacker, defender, combatInfo);

            return new UnitCombatResults(attacker, defender, results.Item1, results.Item2, combatInfo);
        }

        public UnitCombatResults EstimateRangedAttackResults(IUnit attacker, IUnit defender, IHexCell location) {
            if(attacker == null) {
                throw new ArgumentNullException("attacker");
            }
            if(defender == null) {
                throw new ArgumentNullException("defender");
            }
            if(location == null) {
                throw new ArgumentNullException("location");
            }

            var combatInfo = CombatInfoLogic.GetAttackInfo(attacker, defender, location, CombatType.Ranged);

            UniRx.Tuple<int, int> results = CombatCalculator.CalculateCombat(attacker, defender, combatInfo);

            return new UnitCombatResults(attacker, defender, results.Item1, results.Item2, combatInfo);
        }

        #endregion

        #endregion

    }

}
