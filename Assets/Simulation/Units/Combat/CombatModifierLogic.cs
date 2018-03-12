using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;
using Assets.Simulation.Improvements;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Units.Combat {

    public class CombatModifierLogic : ICombatModifierLogic {

        #region instance fields and properties

        private IUnitConfig Config;

        private IRiverCanon RiverCanon;

        private IImprovementLocationCanon ImprovementLocationCanon;

        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;

        private ICivilizationHappinessLogic CivilizationHappinessLogic;

        private ICivilizationConfig CivConfig;

        #endregion

        #region constructors

        public CombatModifierLogic(
            IUnitConfig config, IRiverCanon riverCanon,
            IImprovementLocationCanon improvementLocationCanon,
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            ICivilizationHappinessLogic civilizationHappinessLogic,
            ICivilizationConfig civConfig
        ) {
            Config                     = config;
            RiverCanon                 = riverCanon;
            ImprovementLocationCanon   = improvementLocationCanon;
            UnitPossessionCanon        = unitPossessionCanon;
            CivilizationHappinessLogic = civilizationHappinessLogic;
            CivConfig                  = civConfig;
        }

        #endregion

        #region instance methods

        #region from ICombatModifierLogic

        public float GetMeleeDefensiveModifierAtLocation(IUnit attacker, IUnit defender, IHexCell location) {
            float baseModifier = 0f;

            if(defender.Template.BenefitsFromDefensiveTerrain) {
                baseModifier += Config.TerrainMeleeDefensiveness[(int)location.Terrain];
                baseModifier += Config.FeatureMeleeDefensiveness[(int)location.Feature];

                var improvementAtLocation = ImprovementLocationCanon.GetPossessionsOfOwner(location).FirstOrDefault();
                if(improvementAtLocation != null) {
                    baseModifier += improvementAtLocation.Template.DefensiveBonus;
                }
            }
            
            return baseModifier - GetUnhappinessPenalty(defender);
        }

        public float GetMeleeOffensiveModifierAtLocation(IUnit attacker, IUnit defender, IHexCell location) {
            float retval = 0f;

            if(RiverCanon.HasRiver(location)) {
                retval += Config.RiverCrossingAttackModifier;
            }

            return retval - GetUnhappinessPenalty(attacker);
        }

        public float GetRangedDefensiveModifierAtLocation(IUnit attacker, IUnit defender, IHexCell location) {
            float baseModifier = 0f;

            if(defender.Template.BenefitsFromDefensiveTerrain) {
                baseModifier += Config.TerrainRangedDefensiveness[(int)location.Terrain];
                baseModifier += Config.FeatureRangedDefensiveness[(int)location.Feature];

                var improvementAtLocation = ImprovementLocationCanon.GetPossessionsOfOwner(location).FirstOrDefault();
                if(improvementAtLocation != null) {
                    baseModifier += improvementAtLocation.Template.DefensiveBonus;
                }
            }
            
            return baseModifier - GetUnhappinessPenalty(defender);
        }

        public float GetRangedOffensiveModifierAtLocation(IUnit attacker, IUnit defender, IHexCell location) {
            return -GetUnhappinessPenalty(attacker);
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
