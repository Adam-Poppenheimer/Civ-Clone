using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.SocialPolicies;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Cities {

    public class CityHappinessLogic : ICityHappinessLogic {

        #region instance fields and properties

        private ICityConfig                                   Config;
        private IPossessionRelationship<ICity, IBuilding>     BuildingPossessionCanon;
        private ICityModifiers                                CityModifiers;
        private ISocialPolicyCanon                            SocialPolicyCanon;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private ICapitalConnectionLogic                       CapitalConnectionLogic;

        #endregion

        #region constructors
        
        [Inject]
        public CityHappinessLogic(
            ICityConfig config, IPossessionRelationship<ICity, IBuilding> buildingPossessionCanon,
            ICityModifiers cityModifiers, ISocialPolicyCanon socialPolicyCanon,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            ICapitalConnectionLogic capitalConnectionLogic
        ){
            Config                  = config;
            BuildingPossessionCanon = buildingPossessionCanon;
            CityModifiers           = cityModifiers;
            SocialPolicyCanon       = socialPolicyCanon;
            CityPossessionCanon     = cityPossessionCanon;
            CapitalConnectionLogic  = capitalConnectionLogic;
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

            var cityOwner = CityPossessionCanon.GetOwnerOfPossession(city);

            if(CapitalConnectionLogic.IsCityConnectedToCapital(city)) {
                retval += SocialPolicyCanon.GetPolicyBonusesForCiv(cityOwner).Sum(bonuses => bonuses.ConnectedToCapitalHappiness);
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
