using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Cities.Buildings;

namespace Assets.Simulation.Cities {

    public class CityHappinessLogic : ICityHappinessLogic {

        #region instance fields and properties

        private ICityConfig Config;

        private IPossessionRelationship<ICity, IBuilding> BuildingPossessionCanon;

        #endregion

        #region constructors
        
        [Inject]
        public CityHappinessLogic(
            ICityConfig config,
            IPossessionRelationship<ICity, IBuilding> buildingPossessionCanon
        ){
            Config                  = config;
            BuildingPossessionCanon = buildingPossessionCanon;
        }

        #endregion

        #region instance methods

        #region from ICityHappinessLogic

        public int GetLocalHappinessOfCity(ICity city) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }
            
            int retval = 0;

            foreach(var building in BuildingPossessionCanon.GetPossessionsOfOwner(city)) {
                retval += building.Template.LocalHappiness;
            }

            return retval;
        }

        public int GetGlobalHappinessOfCity(ICity city) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }

            int retval = 0;

            foreach(var building in BuildingPossessionCanon.GetPossessionsOfOwner(city)) {
                retval += building.Template.GlobalHappiness;
            }

            return retval;
        }

        public int GetTotalHappinessofCity(ICity city) {
            return GetLocalHappinessOfCity(city) + GetGlobalHappinessOfCity(city);
        }

        public int GetUnhappinessOfCity(ICity city) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }

            int retval = Config.UnhappinessPerCity;

            retval += Mathf.RoundToInt(Config.UnhappinessPerPopulation * city.Population);

            foreach(var building in BuildingPossessionCanon.GetPossessionsOfOwner(city)) {
                retval += building.Template.Unhappiness;
            }

            return retval;
        }

        public int GetNetHappinessOfCity(ICity city) {
            return GetTotalHappinessofCity(city) - GetUnhappinessOfCity(city);
        }

        #endregion

        #endregion

    }

}
