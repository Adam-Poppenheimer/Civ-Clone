using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;
using Assets.Simulation.Improvements;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Units.Promotions;

namespace Assets.Simulation.Units.Combat {

    public class CombatInfoLogic : ICombatInfoLogic {

        #region instance fields and properties

        private IUnitConfig                                   UnitConfig;
        private IRiverCanon                                   RiverCanon;
        private IImprovementLocationCanon                     ImprovementLocationCanon;
        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private ICivilizationHappinessLogic                   CivilizationHappinessLogic;
        private ICivilizationConfig                           CivConfig;
        private IPromotionParser                              PromotionParser;

        #endregion

        #region constructors

        public CombatInfoLogic(
            IUnitConfig config, IRiverCanon riverCanon,
            IImprovementLocationCanon improvementLocationCanon,
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            ICivilizationHappinessLogic civilizationHappinessLogic,
            ICivilizationConfig civConfig, IPromotionParser promotionParser
        ) {
            UnitConfig                     = config;
            RiverCanon                 = riverCanon;
            ImprovementLocationCanon   = improvementLocationCanon;
            UnitPossessionCanon        = unitPossessionCanon;
            CivilizationHappinessLogic = civilizationHappinessLogic;
            CivConfig                  = civConfig;
            PromotionParser            = promotionParser;
        }

        #endregion

        #region instance methods

        #region from ICombatInfoLogic

        public CombatInfo GetMeleeAttackInfo(IUnit attacker, IUnit defender, IHexCell location) {
            var combatInfo = PromotionParser.GetCombatInfo(attacker, defender, location, CombatType.Melee);

            combatInfo.CombatType = CombatType.Melee;

            if(RiverCanon.HasRiver(location) && !combatInfo.AttackerIgnoresAmphibiousPenalty) {
                combatInfo.AttackerCombatModifier += UnitConfig.RiverCrossingAttackModifier;
            }

            if(!combatInfo.DefenderIgnoresDefensiveTerrainBonuses) {
                combatInfo.DefenderCombatModifier += UnitConfig.GetTerrainDefensiveness(location.Terrain);
                combatInfo.DefenderCombatModifier += UnitConfig.GetFeatureDefensiveness(location.Feature);
                combatInfo.DefenderCombatModifier += UnitConfig.GetShapeDefensiveness  (location.Shape  );

                var improvementAtLocation = ImprovementLocationCanon.GetPossessionsOfOwner(location).FirstOrDefault();
                if(improvementAtLocation != null) {
                    combatInfo.DefenderCombatModifier += improvementAtLocation.Template.DefensiveBonus;
                }
            }

            combatInfo.AttackerCombatModifier -= GetUnhappinessPenalty(attacker);
            combatInfo.DefenderCombatModifier -= GetUnhappinessPenalty(defender);

            return combatInfo;
        }

        public CombatInfo GetRangedAttackInfo(IUnit attacker, IUnit defender, IHexCell location) {
            var combatInfo = PromotionParser.GetCombatInfo(attacker, defender, location, CombatType.Ranged);

            combatInfo.CombatType = CombatType.Ranged;

            if(!combatInfo.DefenderIgnoresDefensiveTerrainBonuses) {
                combatInfo.DefenderCombatModifier += UnitConfig.GetTerrainDefensiveness(location.Terrain);
                combatInfo.DefenderCombatModifier += UnitConfig.GetFeatureDefensiveness(location.Feature);
                combatInfo.DefenderCombatModifier += UnitConfig.GetShapeDefensiveness  (location.Shape  );

                var improvementAtLocation = ImprovementLocationCanon.GetPossessionsOfOwner(location).FirstOrDefault();
                if(improvementAtLocation != null) {
                    combatInfo.DefenderCombatModifier += improvementAtLocation.Template.DefensiveBonus;
                }
            }

            combatInfo.AttackerCombatModifier -= GetUnhappinessPenalty(attacker);
            combatInfo.DefenderCombatModifier -= GetUnhappinessPenalty(defender);

            return combatInfo;
        }

        #endregion

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
