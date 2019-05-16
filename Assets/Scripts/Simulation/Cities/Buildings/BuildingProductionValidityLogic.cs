using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Cities.Buildings {

    /// <summary>
    /// The standard implementation for IBuildingProductionValidityLogic.
    /// </summary>
    public class BuildingProductionValidityLogic : IBuildingProductionValidityLogic {

        #region instance fields and properties

        private List<IBuildingTemplate>                       AvailableTemplates;
        private List<IBuildingRestriction>                    BuildingRestrictions;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;


        #endregion

        #region constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="availableTemplates"></param>
        /// <param name="buildingPossessionCanon"></param>
        [Inject]
        public BuildingProductionValidityLogic(
            List<IBuildingTemplate> availableTemplates, List<IBuildingRestriction> buildingRestrictions,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon
        ){
            AvailableTemplates   = availableTemplates;
            BuildingRestrictions = buildingRestrictions;
            CityPossessionCanon  = cityPossessionCanon;
        }

        #endregion

        #region instance methods

        #region from IBuildingValidityLogic

        /// <inheritdoc/>
        public IEnumerable<IBuildingTemplate> GetTemplatesValidForCity(ICity city) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }

            return AvailableTemplates.Where(template => IsTemplateValidForCity(template, city));
        }

        /// <inheritdoc/>
        public bool IsTemplateValidForCity(IBuildingTemplate template, ICity city) {
            if(template == null) {
                throw new ArgumentNullException("template");
            }else if(city == null) {
                throw new ArgumentNullException("city");
            }
            
            var cityOwner = CityPossessionCanon.GetOwnerOfPossession(city);

            return template.CanBeConstructed
                && BuildingRestrictions.All(restriction => restriction.IsTemplateValidForCity(template, city, cityOwner));
        }

        #endregion

        #endregion
        
    }

}
