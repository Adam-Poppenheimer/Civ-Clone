using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation.Units;
using Assets.Simulation.Units.Combat;
using Assets.Simulation.Cities;

namespace Assets.UI.Units {

    public class CombatSummaryDisplay : MonoBehaviour {

        #region instance fields and properties

        public IUnit AttackingUnit { get; set; }

        public IUnit DefendingUnit { get; set; }

        public bool IsMeleeAttack { get; set; }

        [SerializeField] private Text   AttackerNameField            = null;
        [SerializeField] private Text   AttackerDamageSufferedField  = null;
        [SerializeField] private Text   AttackerHealthLeftField      = null;
        [SerializeField] private Slider AttackerExpectedHealthSlider = null;

        [SerializeField] private Text   DefenderNameField            = null;
        [SerializeField] private Text   DefenderDamageSufferedField  = null;
        [SerializeField] private Text   DefenderHealthLeftField      = null;
        [SerializeField] private Slider DefenderExpectedHealthSlider = null;



        private ICombatEstimator   CombatEstimator;
        private IUnitPositionCanon UnitPositionCanon;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            ICombatEstimator combatEstimator, IUnitPositionCanon unitPositionCanon
        ) {
            CombatEstimator   = combatEstimator;
            UnitPositionCanon = unitPositionCanon;
        }

        #region Unity messages

        private void OnEnable() {
            Refresh();
        }

        private void OnDisable() {
            AttackingUnit = null;
            DefendingUnit = null;
        }

        #endregion

        public void Refresh() {
            if(AttackingUnit != null && DefendingUnit != null) {
                var defenderLocation = UnitPositionCanon.GetOwnerOfPossession(DefendingUnit);

                var estimatedCombat = IsMeleeAttack
                    ? CombatEstimator.EstimateMeleeAttackResults (AttackingUnit, DefendingUnit, defenderLocation)
                    : CombatEstimator.EstimateRangedAttackResults(AttackingUnit, DefendingUnit, defenderLocation);

                AttackerNameField          .text = AttackingUnit.Name;
                AttackerDamageSufferedField.text = estimatedCombat.DamageToAttacker.ToString();
                AttackerHealthLeftField    .text = (AttackingUnit.CurrentHitpoints - estimatedCombat.DamageToAttacker).ToString();

                AttackerExpectedHealthSlider.minValue = 0;
                AttackerExpectedHealthSlider.maxValue = AttackingUnit.MaxHitpoints;
                AttackerExpectedHealthSlider.value = AttackingUnit.CurrentHitpoints - estimatedCombat.DamageToAttacker;


                DefenderNameField          .text = DefendingUnit.Name;
                DefenderDamageSufferedField.text = estimatedCombat.DamageToDefender.ToString();
                DefenderHealthLeftField    .text = (DefendingUnit.CurrentHitpoints - estimatedCombat.DamageToDefender).ToString();

                DefenderExpectedHealthSlider.minValue = 0;
                DefenderExpectedHealthSlider.maxValue = DefendingUnit.MaxHitpoints;
                DefenderExpectedHealthSlider.value = DefendingUnit.CurrentHitpoints - estimatedCombat.DamageToDefender;
            }
        }

        #endregion

    }

}
