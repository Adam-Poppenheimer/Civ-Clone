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

        /// <inheritdoc/>
        /// <remarks>
        /// Unlike other factory classes, this factory checks both technical and mechanical
        /// restrictions to city placement. It calls into
        /// <ref>IBuildingProductionValidityLogic.IsTemplateValidForCity</ref> and
        /// <ref>IBuildingPossessionCanon.GetBuildingsInCity</ref> to make sure that the
        /// argued template is valid. This inconsistency should probably be resolved, though
        /// it's not clear whether BuildingFactory is a model or an aberration.
        /// </remarks>
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

        /// <summary>
        /// Creates a city with the given template in the given city, making sure to assign
        /// that building to its city.
        /// </summary>
        /// <param name="template">The template of the new building</param>
        /// <param name="city">The city to place the new building in</param>
        /// <returns>The newly-created building</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when either template or city are null</exception>
        /// <exception cref="BuildingCreationException">Thrown when <ref>CanConstructTemplateInCity</ref> would return false on the arguments</exception>
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
