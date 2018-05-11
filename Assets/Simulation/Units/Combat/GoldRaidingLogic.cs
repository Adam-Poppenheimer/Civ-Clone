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

        #endregion

        #region constructors

        [Inject]
        public GoldRaidingLogic(
            IPossessionRelationship<ICivilization, IUnit> unitPossessionLogic,
            UnitSignals signals
        ){
            UnitPossessionLogic = unitPossessionLogic;

            signals.MeleeCombatWithUnitSignal.Subscribe(OnMeleeCombatWithUnit);
        }

        #endregion

        #region instance methods

        public void OnMeleeCombatWithUnit(UnitCombatResults results) {
            if(results.Defender.Type != UnitType.City) {
                return;
            }

            var attackerOwner = UnitPossessionLogic.GetOwnerOfPossession(results.Attacker);
            var defenderOwner = UnitPossessionLogic.GetOwnerOfPossession(results.Defender);

            var goldStolen = Mathf.Min(
                results.DamageToDefender * results.InfoOfAttack.Attacker.GoldRaidingPercentage,
                defenderOwner.GoldStockpile
            );

            defenderOwner.GoldStockpile -= Mathf.FloorToInt(goldStolen);
            attackerOwner.GoldStockpile += Mathf.FloorToInt(goldStolen);
        }

        #endregion
        
    }

}
