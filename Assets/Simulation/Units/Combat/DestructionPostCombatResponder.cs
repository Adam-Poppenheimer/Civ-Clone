using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units.Combat {

    public class DestructionPostCombatResponder : IPostCombatResponder {

        #region instance fields and properties

        private IUnitPositionCanon                            UnitPositionCanon;
        private IUnitConfig                                   UnitConfig;
        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public DestructionPostCombatResponder(
            IUnitPositionCanon unitPositionCanon, IUnitConfig unitConfig,
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon
        ) {
            UnitPositionCanon   = unitPositionCanon;
            UnitConfig          = unitConfig;
            UnitPossessionCanon = unitPossessionCanon;
        }

        #endregion

        #region instance methods

        #region from ICombatDestructionLogic

        public void RespondToCombat(IUnit attacker, IUnit defender, CombatInfo combatInfo) {
            if(attacker.CurrentHitpoints <= 0 && attacker.Type != UnitType.City) {
                DestroyUnit(attacker);
            }

            if(defender.CurrentHitpoints <= 0) {
                if(defender.Type == UnitType.City) {
                    defender.CurrentHitpoints = 1;

                } else {
                    var defenderLocation = UnitPositionCanon.GetOwnerOfPossession(defender);

                    var attackerOwner = UnitPossessionCanon.GetOwnerOfPossession(attacker);

                    if( combatInfo.CombatType == CombatType.Melee &&
                        UnitConfig.CapturableTemplates.Contains(defender.Template) &&
                        UnitPossessionCanon.CanChangeOwnerOfPossession(defender, attackerOwner)
                    ) {
                        UnitPossessionCanon.ChangeOwnerOfPossession(defender, attackerOwner);
                    }else {
                        DestroyUnit(defender);
                    }

                    if(attacker.CurrentHitpoints > 0 && combatInfo.CombatType == CombatType.Melee) {
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
