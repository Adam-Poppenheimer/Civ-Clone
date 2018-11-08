using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Cities.Buildings {

    public class OtherBuildingsBuildingRestriction : IBuildingRestriction {

        #region instance fields and properties

        private IPossessionRelationship<ICity, IBuilding>     BuildingPossessionCanon;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public OtherBuildingsBuildingRestriction(
            IPossessionRelationship<ICity, IBuilding> buildingPossessionCanon,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon
        ) {
            BuildingPossessionCanon = buildingPossessionCanon;
            CityPossessionCanon     = cityPossessionCanon;
        }

        #endregion

        #region instance methods

        #region from IBuildingRestriction

        public bool IsTemplateValidForCity(IBuildingTemplate template, ICity city, ICivilization cityOwner) {
            var templatesAlreadyThere = BuildingPossessionCanon.GetPossessionsOfOwner(city).Select(building => building.Template);

            bool templateAlreadyExists = templatesAlreadyThere.Any(
                existingTemplate => existingTemplate == template
            );

            bool allPrerequisitesExist = template.PrerequisiteBuildings.All(
                prerequisite => templatesAlreadyThere.Contains(prerequisite)
            );

            bool meetsGlobalBuildingPrerequisites = true;

            if(template.GlobalPrerequisiteBuildings.Any()) {
                meetsGlobalBuildingPrerequisites = template.GlobalPrerequisiteBuildings.All(
                    globalPrereq => DoAllCitiesHaveTemplate(globalPrereq, cityOwner)
                );
            }

            return meetsGlobalBuildingPrerequisites && !templateAlreadyExists && allPrerequisitesExist;
        }

        #endregion

        private bool DoAllCitiesHaveTemplate(IBuildingTemplate template, ICivilization owner) {
            var allCities = CityPossessionCanon.GetPossessionsOfOwner(owner);

            foreach(var city in allCities) {
                var templatesOf = BuildingPossessionCanon.GetPossessionsOfOwner(city).Select(building => building.Template);

                if(!templatesOf.Contains(template)) {
                    return false;
                }
            }
            return true;
        }

        #endregion
        
    }

}
