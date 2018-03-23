using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.HexMap;
using Assets.Simulation.SpecialtyResources;

namespace Assets.Simulation.Improvements {

    public class ImprovementValidityLogic : IImprovementValidityLogic {

        #region instance fields and properties

        private IPossessionRelationship<IHexCell, IResourceNode> NodePositionCanon;

        private IPossessionRelationship<IHexCell, ICity> CityLocationCanon;

        #endregion

        #region constructors

        [Inject]
        public ImprovementValidityLogic(
            IPossessionRelationship<IHexCell, IResourceNode> nodePositionCanon,
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon
        ){
            NodePositionCanon = nodePositionCanon;
            CityLocationCanon = cityLocationCanon;
        }

        #endregion

        #region instance methods

        #region from IImprovementValidityLogic

        public bool IsTemplateValidForCell(IImprovementTemplate template, IHexCell cell) {
            if(template == null) {
                throw new ArgumentNullException("template");
            }else if(cell == null) {
                throw new ArgumentNullException("cell");
            }

            if(CityLocationCanon.GetPossessionsOfOwner(cell).Count() > 0) {
                return false;
            }

            var nodeAtCell = NodePositionCanon.GetPossessionsOfOwner(cell).FirstOrDefault();
            if(nodeAtCell != null && nodeAtCell.Resource.Extractor == template) {
                return true;
            }else if(template.RequiresResourceToExtract) {
                return false;
            }

            return 
                (template.RestrictedToTerrains.Count() == 0 || template.RestrictedToTerrains.Contains(cell.Terrain)) &&
                (template.RestrictedToFeatures.Count() == 0 || template.RestrictedToFeatures.Contains(cell.Feature)) && 
                (template.RestrictedToShapes  .Count() == 0 || template.RestrictedToShapes  .Contains(cell.Shape  ));
        }

        #endregion

        #endregion

    }

}
