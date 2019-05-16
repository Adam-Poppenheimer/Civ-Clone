using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Cities.Buildings {

    public class WonderBuildingRestriction : IBuildingRestriction {

        #region instance fields and properties

        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private IPossessionRelationship<ICity, IBuilding>     BuildingPossessionCanon;
        private ICityFactory                                  CityFactory;

        #endregion

        #region constructors

        [Inject]
        public WonderBuildingRestriction(
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IPossessionRelationship<ICity, IBuilding> buildingPossessionCanon,
            ICityFactory cityFactory
        ) {
            CityPossessionCanon     = cityPossessionCanon;
            BuildingPossessionCanon = buildingPossessionCanon;
            CityFactory             = cityFactory;
        }

        #endregion

        #region instance methods

        #region from IBuildingRestriction

        public bool IsTemplateValidForCity(IBuildingTemplate template, ICity city, ICivilization cityOwner) {
            if(template.Type != BuildingType.NationalWonder && template.Type != BuildingType.WorldWonder) {
                return true;
            }

            var citiesOfOwner = CityPossessionCanon.GetPossessionsOfOwner(cityOwner);

            foreach(var otherCity in citiesOfOwner) {
                if(otherCity != city && otherCity.ActiveProject != null && otherCity.ActiveProject.BuildingToConstruct == template) {
                    return false;
                }
            }

            IEnumerable<ICity> citiesToCheck = template.Type == BuildingType.NationalWonder ? citiesOfOwner : CityFactory.AllCities;

            foreach(var otherCity in citiesToCheck) {
                var buildingsOf = BuildingPossessionCanon.GetPossessionsOfOwner(otherCity);

                if(buildingsOf.Any(building => building.Template == template)) {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #endregion
        
    }

}
