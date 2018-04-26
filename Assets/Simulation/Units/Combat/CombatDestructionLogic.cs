﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units.Combat {

    public class CombatDestructionLogic : ICombatDestructionLogic {

        #region instance fields and properties

        private IUnitPositionCanon UnitPositionCanon;

        #endregion

        #region constructors

        [Inject]
        public CombatDestructionLogic(IUnitPositionCanon unitPositionCanon) {
            UnitPositionCanon = unitPositionCanon;
        }

        #endregion

        #region instance methods

        #region from ICombatDestructionLogic

        public void HandleUnitDestructionFromCombat(IUnit attacker, IUnit defender, CombatInfo combatInfo) {
            if(attacker.Hitpoints <= 0 && attacker.Type != UnitType.City) {
                DestroyUnit(attacker);
            }

            if(defender.Hitpoints <= 0) {
                if(defender.Type == UnitType.City) {
                    defender.Hitpoints = 1;
                }else {
                    var defenderLocation = UnitPositionCanon.GetOwnerOfPossession(defender);

                    DestroyUnit(defender);

                    if(attacker.Hitpoints > 0 && combatInfo.CombatType == CombatType.Melee) {
                        attacker.CurrentPath = new List<IHexCell>() { defenderLocation };
                        attacker.PerformMovement(false);
                    }
                }
            }
        }

        #endregion

        private void DestroyUnit(IUnit unit) {
            UnitPositionCanon.ChangeOwnerOfPossession(unit, null);

            unit.Destroy();
        }

        #endregion

    }

}
