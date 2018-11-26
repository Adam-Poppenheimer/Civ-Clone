using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;
using Assets.Simulation.Improvements;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Units.Combat {

    public class CombatInfoLogic : ICombatInfoLogic {

        #region instance fields and properties

        private IUnitConfig                                   UnitConfig;
        private IRiverCanon                                   RiverCanon;
        private IImprovementLocationCanon                     ImprovementLocationCanon;
        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private ICivilizationHappinessLogic                   CivilizationHappinessLogic;
        private ICivilizationConfig                           CivConfig;
        private IUnitFortificationLogic                       FortificationLogic;
        private ICombatAuraLogic                              CombatAuraLogic;
        private ICityCombatModifierLogic                      CityCombatModifierLogic;

        #endregion

        #region constructors

        public CombatInfoLogic(
            IUnitConfig config, IRiverCanon riverCanon,
            IImprovementLocationCanon improvementLocationCanon,
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            ICivilizationHappinessLogic civilizationHappinessLogic,
            ICivilizationConfig civConfig, IUnitFortificationLogic fortificationLogic,
            ICombatAuraLogic combatAuraLogic, ICityCombatModifierLogic cityCombatModifierLogic
        ) {
            UnitConfig                 = config;
            RiverCanon                 = riverCanon;
            ImprovementLocationCanon   = improvementLocationCanon;
            UnitPossessionCanon        = unitPossessionCanon;
            CivilizationHappinessLogic = civilizationHappinessLogic;
            CivConfig                  = civConfig;
            FortificationLogic         = fortificationLogic;
            CombatAuraLogic            = combatAuraLogic;
            CityCombatModifierLogic    = cityCombatModifierLogic;
        }

        #endregion

        #region instance methods

        #region from ICombatInfoLogic

        public CombatInfo GetAttackInfo(IUnit attacker, IUnit defender, IHexCell location, CombatType combatType) {
            if(attacker == null) {
                throw new ArgumentNullException("attacker");
            }

            if(defender == null) {
                throw new ArgumentNullException("defender");
            }

            if(location == null) {
                throw new ArgumentNullException("location");
            }

            var combatInfo = new CombatInfo();

            combatInfo.CombatType = combatType;

            ApplyAllModifiersToInfo(attacker, defender, location, combatType, combatInfo);

            if(combatType == CombatType.Melee && RiverCanon.HasRiver(location) && !attacker.CombatSummary.IgnoresAmphibiousPenalty) {
                combatInfo.AttackerCombatModifier += UnitConfig.RiverCrossingAttackModifier;
            }

            if(!defender.CombatSummary.IgnoresDefensiveTerrainBonus) {
                combatInfo.DefenderCombatModifier += UnitConfig.GetTerrainDefensiveness   (location.Terrain);
                combatInfo.DefenderCombatModifier += UnitConfig.GetVegetationDefensiveness(location.Vegetation);
                combatInfo.DefenderCombatModifier += UnitConfig.GetShapeDefensiveness     (location.Shape  );

                var improvementAtLocation = ImprovementLocationCanon.GetPossessionsOfOwner(location).FirstOrDefault();
                if(improvementAtLocation != null) {
                    combatInfo.DefenderCombatModifier += improvementAtLocation.Template.DefensiveBonus;
                }

                combatInfo.DefenderCombatModifier += FortificationLogic.GetFortificationModifierForUnit(defender);
            }

            CombatAuraLogic        .ApplyAurasToCombat        (attacker, defender, combatInfo);
            CityCombatModifierLogic.ApplyCityModifiersToCombat(attacker, defender, combatInfo);

            combatInfo.AttackerCombatModifier -= GetUnhappinessPenalty(attacker);
            combatInfo.DefenderCombatModifier -= GetUnhappinessPenalty(defender);

            return combatInfo;
        }

        #endregion

        private void ApplyAllModifiersToInfo(
            IUnit attacker, IUnit defender, IHexCell location, CombatType combatType, CombatInfo info
        ) {
            foreach(var attackerModifier in attacker.CombatSummary.ModifiersWhenAttacking) {
                if(attackerModifier.DoesModifierApply(attacker, defender, location, combatType)) {
                    info.AttackerCombatModifier += attackerModifier.Modifier;
                }
            }

            foreach(var defenderModifiers in defender.CombatSummary.ModifiersWhenDefending) {
                if(defenderModifiers.DoesModifierApply(defender, attacker, location, combatType)) {
                    info.DefenderCombatModifier += defenderModifiers.Modifier;
                }
            }
        }

        private float GetUnhappinessPenalty(IUnit unit) {
            var unitOwner = UnitPossessionCanon.GetOwnerOfPossession(unit);

            var ownerHappiness = CivilizationHappinessLogic.GetNetHappinessOfCiv(unitOwner);

            if(ownerHappiness < 0) {
                return ownerHappiness * CivConfig.ModifierLossPerUnhappiness;
            }else {
                return 0f;
            }
        }

        #endregion
        
    }

}
