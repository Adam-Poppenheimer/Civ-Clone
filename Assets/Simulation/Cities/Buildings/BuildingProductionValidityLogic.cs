using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;
using Assets.Simulation.Improvements;

namespace Assets.Simulation.Cities.Buildings {

    /// <summary>
    /// The standard implementation for IBuildingProductionValidityLogic.
    /// </summary>
    public class BuildingProductionValidityLogic : IBuildingProductionValidityLogic {

        #region instance fields and properties

        private List<IBuildingTemplate>                          AvailableTemplates;
        private IPossessionRelationship<ICity, IBuilding>        BuildingPossessionCanon;
        private IPossessionRelationship<ICivilization, ICity>    CityPossessionCanon;
        private IFreeResourcesLogic                              FreeResourcesLogic;
        private IPossessionRelationship<IHexCell, ICity>         CityLocationCanon;
        private IHexGrid                                         Grid;
        private IRiverCanon                                      RiverCanon;
        private IPossessionRelationship<ICity, IHexCell>         CellPossessionCanon;
        private IPossessionRelationship<IHexCell, IResourceNode> NodeLocationCanon;
        private IImprovementLocationCanon                        ImprovementLocationCanon;


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
            IFreeResourcesLogic freeResourcesLogic,
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon,
            IHexGrid grid, IRiverCanon riverCanon,
            IPossessionRelationship<ICity, IHexCell> cellPossessionCanon,
            IPossessionRelationship<IHexCell, IResourceNode> nodeLocationCanon,
            IImprovementLocationCanon improvementLocationCanon
        ){
            AvailableTemplates       = availableTemplates;
            BuildingPossessionCanon  = buildingPossessionCanon;
            CityPossessionCanon      = cityPossessionCanon;
            FreeResourcesLogic       = freeResourcesLogic;
            CityLocationCanon        = cityLocationCanon;
            Grid                     = grid;
            RiverCanon               = riverCanon;
            CellPossessionCanon      = cellPossessionCanon;
            NodeLocationCanon        = nodeLocationCanon;
            ImprovementLocationCanon = improvementLocationCanon;
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
            foreach(var resource in template.ResourcesConsumed) {
                if(FreeResourcesLogic.GetFreeCopiesOfResourceForCiv(resource, cityOwner) <= 0) {
                    return false;
                }
            }

            var templatesAlreadyThere = BuildingPossessionCanon.GetPossessionsOfOwner(city).Select(building => building.Template);

            foreach(var prerequisiteBuilding in template.PrerequisiteBuildings) {
                if(!templatesAlreadyThere.Contains(prerequisiteBuilding)) {
                    return false;
                }
            }

            var cellsOfCity = CellPossessionCanon.GetPossessionsOfOwner(city);
            bool resourcePrerequisiteMet = template.PrerequisiteResourcesNearCity.Count() == 0;

            foreach(var prerequisiteResource in template.PrerequisiteResourcesNearCity) {

                foreach(var cell in cellsOfCity) {
                    IResourceNode nodeOnCell = NodeLocationCanon.GetPossessionsOfOwner(cell).FirstOrDefault();
                    if(nodeOnCell != null && nodeOnCell.Resource == prerequisiteResource) {
                        resourcePrerequisiteMet = true;
                        break;
                    }
                }

                if(resourcePrerequisiteMet) {
                    break;
                }
            }

            if(!resourcePrerequisiteMet) {
                return false;
            }

            bool improvementPrerequisiteMet = template.PrerequisiteImprovementsNearCity.Count() == 0;

            foreach(IImprovementTemplate improvementPrerequisite in template.PrerequisiteImprovementsNearCity) {

                foreach(var cell in cellsOfCity) {
                    IImprovement improvementOnCell = ImprovementLocationCanon.GetPossessionsOfOwner(cell).FirstOrDefault();
                    if(improvementOnCell != null && improvementOnCell.Template == improvementPrerequisite) {
                        improvementPrerequisiteMet = true;
                        break;
                    }
                }

                if(improvementPrerequisiteMet) {
                    break;
                }
            }

            if(!improvementPrerequisiteMet) {
                return false;
            }

            if(templatesAlreadyThere.Contains(template)) {
                return false;
            }

            var cityLocation = CityLocationCanon.GetOwnerOfPossession(city);

            if( template.RequiresAdjacentRiver &&
                Grid.GetNeighbors(cityLocation).Where(neighbor => RiverCanon.HasRiver(neighbor)).Count() <= 0
            ){
                return false;
            }

            if( template.RequiresCoastalCity && 
                Grid.GetNeighbors(cityLocation).Where(neighbor => neighbor.Terrain.IsWater()).Count() <= 0
            ){
                return false;
            }

            return true;
        }

        #endregion

        #endregion
        
    }

}
