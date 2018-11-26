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

        private ICityConfig                               Config;
        private IPossessionRelationship<ICity, IBuilding> BuildingPossessionCanon;
        private ICityModifiers                            CityModifiers;

        #endregion

        #region constructors
        
        [Inject]
        public CityHappinessLogic(
            ICityConfig config, IPossessionRelationship<ICity, IBuilding> buildingPossessionCanon,
            ICityModifiers cityModifiers
        ){
            Config                  = config;
            BuildingPossessionCanon = buildingPossessionCanon;
            CityModifiers           = cityModifiers;
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

            retval += Mathf.FloorToInt(
                city.Population * CityModifiers.PerPopulationHappiness.GetValueForCity(city)
            );

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

            retval += Mathf.FloorToInt(
                city.Population * CityModifiers.PerPopulationUnhappiness.GetValueForCity(city)
            );

            return retval;
        }

        public int GetNetHappinessOfCity(ICity city) {
            return GetTotalHappinessofCity(city) - GetUnhappinessOfCity(city);
        }

        #endregion

        #endregion

    }

}
