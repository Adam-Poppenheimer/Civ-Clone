using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Units.Promotions;

namespace Assets.Simulation.Units.Combat {

    public class CombatAuraLogic : ICombatAuraLogic {

        #region instance fields and properties

        private IUnitConfig                                   UnitConfig;
        private IUnitPositionCanon                            UnitPositionCanon;
        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private IHexGrid                                      Grid;

        #endregion

        #region constructors

        [Inject]
        public CombatAuraLogic(
            IUnitConfig unitConfig, IUnitPositionCanon unitPositionCanon,
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            IHexGrid grid
        ) {
            UnitConfig          = unitConfig;
            UnitPositionCanon   = unitPositionCanon;
            UnitPossessionCanon = unitPossessionCanon;
            Grid                = grid;
        }

        #endregion

        #region instance methods

        #region from ICombatAuraLogic

        public void ApplyAurasToCombat(
            IUnit attacker, IUnit defender, CombatInfo combatInfo
        ) {
            var attackerLocation = UnitPositionCanon.GetOwnerOfPossession(attacker);
            var defenderLocation = UnitPositionCanon.GetOwnerOfPossession(defender);

            float attackerAuraStrength = 0f, defenderAuraStrength = 0f;

            var attackerAllyAuraModifiers = GetNearbyAllies(attacker, UnitConfig.AuraRange).SelectMany(
                ally => ally.CombatSummary.AuraModifiersWhenAttacking
            );

            foreach(var auraModifier in attackerAllyAuraModifiers) {
                if(auraModifier.DoesModifierApply(attacker, defender, attackerLocation, combatInfo.CombatType)) {
                    attackerAuraStrength += auraModifier.Modifier;
                }
            }

            var defenderAllyAuraModifiers = GetNearbyAllies(defender, UnitConfig.AuraRange).SelectMany(
                ally => ally.CombatSummary.AuraModifiersWhenDefending
            );

            foreach(var auraModifier in defenderAllyAuraModifiers) {
                if(auraModifier.DoesModifierApply(defender, attacker, defenderLocation, combatInfo.CombatType)) {
                    defenderAuraStrength += auraModifier.Modifier;
                }
            }

            combatInfo.AttackerCombatModifier += attackerAuraStrength;
            combatInfo.DefenderCombatModifier += defenderAuraStrength;
        }

        #endregion

        private IEnumerable<IUnit> GetNearbyAllies(IUnit unit, int maxRange) {
            var unitLocation = UnitPositionCanon  .GetOwnerOfPossession(unit);
            var unitOwner    = UnitPossessionCanon.GetOwnerOfPossession(unit);

            var retval = new List<IUnit>();

            if(unitLocation != null && unitOwner != null) {
                var cellsInRange = Grid.GetCellsInRadius(unitLocation, maxRange);

                foreach(var nearbyUnit in cellsInRange.SelectMany(cell => UnitPositionCanon.GetPossessionsOfOwner(cell))) {
                    if(UnitPossessionCanon.GetOwnerOfPossession(nearbyUnit) == unitOwner) {
                        retval.Add(nearbyUnit);
                    }
                }
            }

            return retval;
        }

        #endregion

    }

}
