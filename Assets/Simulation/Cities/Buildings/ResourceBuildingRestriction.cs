using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.MapResources;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.Cities.Buildings {

    public class ResourceBuildingRestriction : IBuildingRestriction {

        #region instance fields and properties

        private IFreeResourcesLogic                              FreeResourcesLogic;
        private IPossessionRelationship<ICity, IHexCell>         CellPossessionCanon;
        private IPossessionRelationship<IHexCell, IResourceNode> NodeLocationCanon;

        #endregion

        #region constructors

        [Inject]
        public ResourceBuildingRestriction(
            IFreeResourcesLogic freeResourcesLogic,
            IPossessionRelationship<ICity, IHexCell> cellPossessionCanon,
            IPossessionRelationship<IHexCell, IResourceNode> nodeLocationCanon
        ) {
            FreeResourcesLogic  = freeResourcesLogic;
            CellPossessionCanon = cellPossessionCanon;
            NodeLocationCanon   = nodeLocationCanon;
        }

        #endregion

        #region instance methods

        #region from IBuildingRestriction

        public bool IsTemplateValidForCity(
            IBuildingTemplate template, ICity city, ICivilization cityOwner
        ) {
            foreach(var resource in template.ResourcesConsumed) {
                if(FreeResourcesLogic.GetFreeCopiesOfResourceForCiv(resource, cityOwner) <= 0) {
                    return false;
                }
            }

            var cellsOfCity = CellPossessionCanon.GetPossessionsOfOwner(city);

            if(template.PrerequisiteResourcesNearCity.Any() && !template.PrerequisiteResourcesNearCity.Any(
                prereq => cellsOfCity.Any(cell => DoesCellHaveNodeOfResource(cell, prereq))
            )) {
                return false;
            }

            return true;
        }

        #endregion

        private bool DoesCellHaveNodeOfResource(IHexCell cell, IResourceDefinition resource) {
            var nodeAtCell = NodeLocationCanon.GetPossessionsOfOwner(cell).FirstOrDefault();

            return nodeAtCell != null && nodeAtCell.Resource == resource;
        }

        #endregion
        
    }

}
