using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Units.Combat {

    public class GoldRaidingLogic {

        #region instance fields and properties

        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionLogic;
        private ICivModifiers                                 CivModifiers;

        #endregion

        #region constructors

        [Inject]
        public GoldRaidingLogic(
            IPossessionRelationship<ICivilization, IUnit> unitPossessionLogic,
            ICivModifiers civModifiers, UnitSignals signals
        ){
            UnitPossessionLogic = unitPossessionLogic;
            CivModifiers        = civModifiers;

            signals.MeleeCombatWithUnit .Subscribe(OnMeleeCombatWithUnit);
            signals.RangedCombatWithUnit.Subscribe(HandleBounty);
        }

        #endregion

        #region instance methods

        public void OnMeleeCombatWithUnit(UnitCombatResults results) {
            HandleBounty(results);

            if(results.Defender.Type != UnitType.City) {
                return;
            }

            var attackerOwner = UnitPossessionLogic.GetOwnerOfPossession(results.Attacker);
            var defenderOwner = UnitPossessionLogic.GetOwnerOfPossession(results.Defender);

            var goldStolen = Mathf.Min(
                results.DamageToDefender * results.Attacker.CombatSummary.GoldRaidingPercentage,
                defenderOwner.GoldStockpile
            );

            defenderOwner.GoldStockpile -= Mathf.FloorToInt(goldStolen);
            attackerOwner.GoldStockpile += Mathf.FloorToInt(goldStolen);
        }

        private void HandleBounty(UnitCombatResults results) {
            if(results.Attacker.CurrentHitpoints > 0 && results.Defender.CurrentHitpoints <= 0) {
                var attackerOwner = UnitPossessionLogic.GetOwnerOfPossession(results.Attacker);

                float modifier = CivModifiers.GoldBountyPerProduction.GetValueForCiv(attackerOwner);

                attackerOwner.GoldStockpile += Mathf.RoundToInt(results.Defender.Template.ProductionCost * modifier);

            }else if(results.Defender.CurrentHitpoints > 0 && results.Attacker.CurrentHitpoints <= 0) {
                var defenderOwner = UnitPossessionLogic.GetOwnerOfPossession(results.Defender);

                float modifier = CivModifiers.GoldBountyPerProduction.GetValueForCiv(defenderOwner);

                defenderOwner.GoldStockpile += Mathf.RoundToInt(results.Attacker.Template.ProductionCost * modifier);
            }
        }

        #endregion
        
    }

}
