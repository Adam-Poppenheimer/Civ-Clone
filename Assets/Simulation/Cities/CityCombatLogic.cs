using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.Cities.Buildings;

namespace Assets.Simulation.Cities {

    public class CityCombatLogic : ICityCombatLogic {

        #region instance fields and properties

        private ICityConfig Config;

        private IPossessionRelationship<ICity, IBuilding> BuildingPossessionCanon;

        #endregion

        #region constructors

        public CityCombatLogic(ICityConfig config,
            IPossessionRelationship<ICity, IBuilding> buildingPossessionCanon
        ){
            Config                  = config;
            BuildingPossessionCanon = buildingPossessionCanon;
        }

        #endregion

        #region instance methods

        #region from ICityCombatLogic

        public int GetCombatStrengthOfCity(ICity city) {
            int retval = Config.BaseCombatStrength + Mathf.RoundToInt(Config.CombatStrengthPerPopulation * city.Population);

            foreach(var building in BuildingPossessionCanon.GetPossessionsOfOwner(city)) {
                retval += building.Template.CityCombatStrengthBonus;
            }

            return retval;
        }

        public int GetMaxHitpointsOfCity(ICity city) {
            int retval = Config.BaseMaxHitPoints + Mathf.RoundToInt(Config.MaxHitPointsPerPopulation * city.Population);

            foreach(var building in BuildingPossessionCanon.GetPossessionsOfOwner(city)) {
                retval += building.Template.CityMaxHitpointBonus;
            }

            return retval;
        }

        public int GetRangedAttackStrengthOfCity(ICity city) {
            return Config.BaseRangedAttackStrength + Mathf.RoundToInt(Config.RangedAttackStrengthPerPopulation * city.Population);
        }

        #endregion

        #endregion
        
    }

}
