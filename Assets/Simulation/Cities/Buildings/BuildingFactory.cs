using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Simulation.Cities.Buildings {

    public class BuildingFactory : IBuildingFactory {

        #region instance fields and properties

        private IBuildingProductionValidityLogic ValidityLogic;
        private IBuildingPossessionCanon PossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public BuildingFactory(IBuildingProductionValidityLogic validityLogic, IBuildingPossessionCanon possessionCanon) {
            ValidityLogic = validityLogic;
            PossessionCanon = possessionCanon;
        }

        #endregion

        #region instance methods

        #region from IBuildingFactory

        public bool CanConstructTemplateInCity(IBuildingTemplate template, ICity city) {
            if(template == null) {
                throw new ArgumentNullException("template");
            }else if(city == null) {
                throw new ArgumentNullException("city");
            }

            var templateIsValid = ValidityLogic.IsTemplateValidForCity(template, city);
            var buildingAlreadyExists = PossessionCanon.GetBuildingsInCity(city).Any(building => building.Template == template);

            return templateIsValid && !buildingAlreadyExists;
        }

        public IBuilding Create(IBuildingTemplate template, ICity city) {
            if(template == null) {
                throw new ArgumentNullException("template");
            }else if(city == null) {
                throw new ArgumentNullException("city");
            }

            if(!CanConstructTemplateInCity(template, city)) {
                throw new BuildingCreationException("A building of this template cannot be constructed in this city");
            }

            var newBuilding = new Building(template);

            if(!PossessionCanon.CanPlaceBuildingInCity(newBuilding, city)) {
                throw new BuildingCreationException("The building produced from this template cannot be placed into this city");
            }
            PossessionCanon.PlaceBuildingInCity(newBuilding, city);

            return newBuilding;
        }        

        #endregion

        #endregion

    }

}
