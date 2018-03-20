using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.Cities.Buildings {

    /// <summary>
    /// The standard implementation for IBuildingProductionValidityLogic.
    /// </summary>
    public class BuildingProductionValidityLogic : IBuildingProductionValidityLogic {

        #region instance fields and properties

        private List<IBuildingTemplate>                       AvailableTemplates;
        private IPossessionRelationship<ICity, IBuilding>     BuildingPossessionCanon;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private IResourceAssignmentCanon                      ResourceAssignmentCanon;
        private IPossessionRelationship<IHexCell, ICity>      CityLocationCanon;
        private IHexGrid                                      Grid;
        private IRiverCanon                                   RiverCanon;

        #endregion

        #region constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="availableTemplates"></param>
        /// <param name="buildingPossessionCanon"></param>
        [Inject]
        public BuildingProductionValidityLogic(
            List<IBuildingTemplate> availableTemplates,
            IPossessionRelationship<ICity, IBuilding> buildingPossessionCanon,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IResourceAssignmentCanon resourceAssignmentCanon,
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon,
            IHexGrid grid, IRiverCanon riverCanon
        ){
            AvailableTemplates      = availableTemplates;
            BuildingPossessionCanon = buildingPossessionCanon;
            CityPossessionCanon     = cityPossessionCanon;
            ResourceAssignmentCanon = resourceAssignmentCanon;
            CityLocationCanon       = cityLocationCanon;
            Grid                    = grid;
            RiverCanon              = riverCanon;
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
            foreach(var resource in template.RequiredResources) {
                if(ResourceAssignmentCanon.GetFreeCopiesOfResourceForCiv(resource, cityOwner) <= 0) {
                    return false;
                }
            }

            var templatesAlreadyThere = BuildingPossessionCanon.GetPossessionsOfOwner(city).Select(building => building.Template);

            foreach(var prequisite in template.PrerequisiteBuildings) {
                if(!templatesAlreadyThere.Contains(prequisite)) {
                    return false;
                }
            }

            if(templatesAlreadyThere.Contains(template)) {
                return false;
            }

            var cityLocation = CityLocationCanon.GetOwnerOfPossession(city);

            if(template.RequiresAdjacentRiver) {
                return Grid.GetNeighbors(cityLocation).Where(neighbor => RiverCanon.HasRiver(neighbor)).Count() > 0;
            }

            return true;
        }

        #endregion

        #endregion
        
    }

}
