using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Cities.Buildings {

    public class BuildingFactory : IBuildingFactory {

        #region instance fields and properties

        private ITemplateValidityLogic ValidityLogic;
        private IBuildingPossessionCanon PossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public BuildingFactory(ITemplateValidityLogic validityLogic, IBuildingPossessionCanon possessionCanon) {
            ValidityLogic = validityLogic;
            PossessionCanon = possessionCanon;
        }

        #endregion

        #region instance methods

        #region from IManagingBuildingFactory

        public bool CanConstructTemplateInCity(IBuildingTemplate template, ICity city) {
            var templateIsValid = ValidityLogic.IsTemplateValidForCity(template, city);
            var buildingAlreadyExists = PossessionCanon.GetBuildingsInCity(city).Any(building => building.Template == template);

            return templateIsValid && !buildingAlreadyExists;
        }

        public IBuilding Create(IBuildingTemplate template, ICity city) {
            if(!CanConstructTemplateInCity(template, city)) {
                throw new InvalidOperationException("A building of this template cannot be constructed in this city");
            }

            var newBuilding = new Building(template);

            if(!PossessionCanon.CanPlaceBuildingInCity(newBuilding, city)) {
                throw new InvalidOperationException("The city produced from this template cannot be placed into this city");
            }
            PossessionCanon.PlaceBuildingInCity(newBuilding, city);

            return newBuilding;
        }        

        #endregion

        #endregion

    }

}
