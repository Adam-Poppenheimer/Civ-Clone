using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Combat;

namespace Assets.Simulation.AI {

    public class UnitComparativeStrengthEstimator : IUnitComparativeStrengthEstimator {

        #region instance fields and properties

        private ICombatInfoLogic CombatInfoLogic;

        #endregion

        #region constructors

        [Inject]
        public UnitComparativeStrengthEstimator(ICombatInfoLogic combatInfoLogic) {
            CombatInfoLogic = combatInfoLogic;
        }

        #endregion

        #region instance methods

        #region from IUnitComparativeStrengthEstimator

        public float EstimateComparativeStrength(IUnit unit, IUnit defender, IHexCell location) {
            if(unit.RangedAttackStrength > 0) {
                var attackInfo = CombatInfoLogic.GetAttackInfo(unit, defender, location, CombatType.Ranged);
                return unit.RangedAttackStrength * (1f + attackInfo.AttackerCombatModifier);

            }else {
                var attackInfo = CombatInfoLogic.GetAttackInfo(unit, defender, location, CombatType.Melee);
                return unit.CombatStrength * (1f + attackInfo.AttackerCombatModifier);
            }
        }

        public float EstimateComparativeDefensiveStrength(IUnit unit, IUnit attacker, IHexCell location) {
            if(attacker.RangedAttackStrength > 0) {
                var attackInfo = CombatInfoLogic.GetAttackInfo(attacker, unit, location, CombatType.Ranged);
                return unit.CombatStrength * (1f + attackInfo.DefenderCombatModifier);

            }else {
                var attackInfo = CombatInfoLogic.GetAttackInfo(attacker, unit, location, CombatType.Melee);
                return unit.CombatStrength * (1f + attackInfo.DefenderCombatModifier);
            }
        }

        #endregion

        #endregion

    }

}
