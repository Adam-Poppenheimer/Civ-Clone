using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.MapResources;
using Assets.Simulation.Cities;

namespace Assets.Simulation.Civilizations {

    public class CivilizationHappinessLogic : ICivilizationHappinessLogic {

        #region instance fields and properties

        private ICivilizationConfig                           Config;
        private IFreeResourcesLogic                           FreeResourcesLogic;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private ICityHappinessLogic                           CityHappinessLogic;
        private List<IResourceDefinition>            AvailableLuxuries;

        #endregion

        #region constructors

        public CivilizationHappinessLogic(
            ICivilizationConfig config, IFreeResourcesLogic freeResourcesLogic,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            ICityHappinessLogic cityHappinessLogic,
            [Inject(Id = "Available Resources")] IEnumerable<IResourceDefinition> availableResources
        ){
            Config              = config;
            FreeResourcesLogic  = freeResourcesLogic;
            CityPossessionCanon = cityPossessionCanon;
            CityHappinessLogic  = cityHappinessLogic;

            AvailableLuxuries = availableResources.Where(resource => resource.Type == ResourceType.Luxury).ToList();
        }

        #endregion

        #region instance methods

        #region from ICivilizationHappinessLogic

        public int GetHappinessOfCiv(ICivilization civ) {
            if(civ == null) {
                throw new ArgumentNullException("civ");
            }

            int retval = Config.BaseHappiness;

            var freeLuxuries = AvailableLuxuries.Where(
                luxury => FreeResourcesLogic.GetFreeCopiesOfResourceForCiv(luxury, civ) > 0
            );

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
