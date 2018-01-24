using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.Cities {

    public class CityCombatLogic : ICityCombatLogic {

        #region instance fields and properties

        private ICityConfig Config;

        #endregion

        #region constructors

        public CityCombatLogic(ICityConfig config) {
            Config = config;
        }

        #endregion

        #region instance methods

        #region from ICityCombatLogic

        public int GetCombatStrengthOfCity(ICity city) {
            return Config.BaseCombatStrength + Mathf.RoundToInt(Config.CombatStrengthPerPopulation * city.Population);
        }

        public int GetMaxHealthOfCity(ICity city) {
            return Config.BaseMaxHealth + Mathf.RoundToInt(Config.MaxHealthPerPopulation * city.Population);
        }

        public int GetRangedAttackStrengthOfCity(ICity city) {
            return Config.BaseRangedAttackStrength + Mathf.RoundToInt(Config.RangedAttackStrengthPerPopulation * city.Population);
        }

        #endregion

        #endregion
        
    }

}
