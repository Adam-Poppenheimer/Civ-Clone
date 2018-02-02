using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.SpecialtyResources;
using Assets.Simulation.Improvements;

namespace Assets.Simulation.HexMap {

    public class TileResourceLogic : ITileResourceLogic {

        #region instance fields and properties

        private IHexGridConfig Config;

        private IPossessionRelationship<IHexCell, IResourceNode> NodePositionCanon;

        private IImprovementLocationCanon ImprovementLocationCanon;

        #endregion

        #region constructors

        [Inject]
        public TileResourceLogic(IHexGridConfig config,
            IPossessionRelationship<IHexCell, IResourceNode> nodePositionCanon,
            IImprovementLocationCanon improvementLocationCanon
        ){
            Config                   = config;
            NodePositionCanon        = nodePositionCanon;
            ImprovementLocationCanon = improvementLocationCanon;
        }

        #endregion

        #region instance methods

        #region from ITileResourceLogic

        public ResourceSummary GetYieldOfCell(IHexCell cell) {
            if(cell == null) {
                throw new ArgumentNullException("cell");
            }

            ResourceSummary retval;

            if(cell.Feature != TerrainFeature.None) {
                retval = Config.FeatureYields[(int)cell.Feature];
            }else {
                retval = Config.TerrainYields[(int)cell.Terrain];
            }

            var nodeAtLocation = NodePositionCanon.GetPossessionsOfOwner(cell).FirstOrDefault();
            if(nodeAtLocation != null) {
                retval += nodeAtLocation.Resource.BonusYield;
            }

            var improvementOnCell = ImprovementLocationCanon.GetPossessionsOfOwner(cell).FirstOrDefault();
            if(improvementOnCell != null) {
                retval += improvementOnCell.Template.BonusYield;
            }

            return retval;
        }

        #endregion

        #endregion
        
    }

}
