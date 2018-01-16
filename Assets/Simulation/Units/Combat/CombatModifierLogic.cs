using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units.Combat {

    public class CombatModifierLogic : ICombatModifierLogic {

        #region instance fields and properties

        private IUnitConfig Config;

        private IRiverCanon RiverCanon;

        #endregion

        #region constructors

        public CombatModifierLogic(IUnitConfig config, IRiverCanon riverCanon) {
            Config     = config;
            RiverCanon = riverCanon;
        }

        #endregion

        #region instance methods

        #region from ICombatModifierLogic

        public float GetMeleeDefensiveModifierAtLocation(IUnit attacker, IUnit defender, IHexCell location) {
            float baseModifier = 0f;

            baseModifier += Config.TerrainMeleeDefensiveness[(int)location.Terrain];
            baseModifier += Config.FeatureMeleeDefensiveness[(int)location.Feature];
            
            return baseModifier;
        }

        public float GetMeleeOffensiveModifierAtLocation(IUnit attacker, IUnit defender, IHexCell location) {
            if(RiverCanon.HasRiver(location)) {
                return Config.RiverCrossingAttackModifier;
            }else {
                return 0f;
            }
        }

        public float GetRangedDefensiveModifierAtLocation(IUnit attacker, IUnit defender, IHexCell location) {
            float baseModifier = 0f;

            baseModifier += Config.TerrainRangedDefensiveness[(int)location.Terrain];
            baseModifier += Config.FeatureRangedDefensiveness[(int)location.Feature];
            
            return baseModifier;
        }

        public float GetRangedOffensiveModifierAtLocation(IUnit attacker, IUnit defender, IHexCell location) {
            return 0f;
        }

        #endregion

        #endregion
        
    }

}
