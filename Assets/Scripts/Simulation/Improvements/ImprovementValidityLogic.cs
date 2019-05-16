using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine.Profiling;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;

namespace Assets.Simulation.Improvements {

    public class ImprovementValidityLogic : IImprovementValidityLogic {

        #region instance fields and properties

        private IPossessionRelationship<IHexCell, IResourceNode> NodePositionCanon;
        private IPossessionRelationship<IHexCell, ICity>         CityLocationCanon;
        private IFreshWaterLogic                                 FreshWaterCanon;

        #endregion

        #region constructors

        [Inject]
        public ImprovementValidityLogic(
            IPossessionRelationship<IHexCell, IResourceNode> nodePositionCanon,
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon,
            IFreshWaterLogic freshWaterCanon
        ){
            NodePositionCanon = nodePositionCanon;
            CityLocationCanon = cityLocationCanon;
            FreshWaterCanon   = freshWaterCanon;
        }

        #endregion

        #region instance methods

        #region from IImprovementValidityLogic

        public bool IsTemplateValidForCell(
            IImprovementTemplate template, IHexCell cell, bool ignoreOwnership
        ) {
            if(template == null) {
                throw new ArgumentNullException("template");
            }else if(cell == null) {
                throw new ArgumentNullException("cell");
            }

            if(!ignoreOwnership && CityLocationCanon.GetPossessionsOfOwner(cell).Count() > 0) {
                return false;
            }

            var nodeAtCell = NodePositionCanon.GetPossessionsOfOwner(cell).FirstOrDefault();
            if(nodeAtCell != null && nodeAtCell.Resource.Extractor == template) {
                return true;

            }else if(template.RequiresResourceToExtract) {
                return false;

            }else if(template.FreshWaterAlwaysEnables && FreshWaterCanon.HasAccessToFreshWater(cell)) {
                return true;
            }

            var retval = cell.Feature == CellFeature.None &&
                   (template.RestrictedToTerrains   .Count() == 0 || template.RestrictedToTerrains   .Contains(cell.Terrain)) &&
                   (template.RestrictedToVegetations.Count() == 0 || template.RestrictedToVegetations.Contains(cell.Vegetation)) && 
                   (template.RestrictedToShapes     .Count() == 0 || template.RestrictedToShapes     .Contains(cell.Shape  ));

            return retval;
        }

        #endregion

        #endregion

    }

}
