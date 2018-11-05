using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Cities.Buildings {

    public class OtherBuildingsBuildingRestriction : IBuildingRestriction {

        #region instance fields and properties

        private IPossessionRelationship<ICity, IBuilding> BuildingPossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public OtherBuildingsBuildingRestriction(
            IPossessionRelationship<ICity, IBuilding> buildingPossessionCanon
        ) {
            BuildingPossessionCanon = buildingPossessionCanon;
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

            return !templateAlreadyExists && allPrerequisitesExist;
        }

        #endregion

        #endregion
        
    }

}
