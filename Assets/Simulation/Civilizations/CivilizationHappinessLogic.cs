using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.SpecialtyResources;
using Assets.Simulation.Cities;

namespace Assets.Simulation.Civilizations {

    public class CivilizationHappinessLogic : ICivilizationHappinessLogic {

        #region instance fields and properties

        private ICivilizationConfig Config;

        private IResourceAssignmentCanon ResourceAssignmentCanon;

        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        private ICityHappinessLogic CityHappinessLogic;

        #endregion

        #region constructors

        public CivilizationHappinessLogic(
            ICivilizationConfig config, IResourceAssignmentCanon resourceAssignmentCanon,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            ICityHappinessLogic cityHappinessLogic
        ){
            Config                  = config;
            ResourceAssignmentCanon = resourceAssignmentCanon;
            CityPossessionCanon     = cityPossessionCanon;
            CityHappinessLogic      = cityHappinessLogic;
        }

        #endregion

        #region instance methods

        #region from ICivilizationHappinessLogic

        public int GetHappinessOfCiv(ICivilization civ) {
            if(civ == null) {
                throw new ArgumentNullException("civ");
            }

            int retval = Config.BaseHappiness;

            var freeLuxuries = ResourceAssignmentCanon.GetAllFreeResourcesForCiv(civ)
                .Where(resource => resource.Type == SpecialtyResourceType.Luxury);

            retval += freeLuxuries.Count() * Config.HappinessPerLuxury;

            foreach(var city in CityPossessionCanon.GetPossessionsOfOwner(civ)) {
                retval += Math.Min(CityHappinessLogic.GetLocalHappinessOfCity(city), city.Population);
                retval += CityHappinessLogic.GetGlobalHappinessOfCity(city);
            }

            return retval;
        }

        public int GetUnhappinessOfCiv(ICivilization civ) {
            if(civ == null) {
                throw new ArgumentNullException("civ");
            }

            int retval = 0;

            foreach(var city in CityPossessionCanon.GetPossessionsOfOwner(civ)) {
                retval += CityHappinessLogic.GetUnhappinessOfCity(city);
            }

            return retval;
        }

        public int GetNetHappinessOfCiv(ICivilization civ) {
            return GetHappinessOfCiv(civ) - GetUnhappinessOfCiv(civ);
        }

        #endregion

        #endregion
        
    }

}
