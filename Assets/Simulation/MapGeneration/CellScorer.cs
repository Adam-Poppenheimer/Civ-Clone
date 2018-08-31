using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;

namespace Assets.Simulation.MapGeneration {

    public class CellScorer : ICellScorer {

        #region instance fields and properties

        private IYieldEstimator                                  YieldEstimator;
        private IMapScorer                                       MapScorer;
        private IPossessionRelationship<IHexCell, IResourceNode> NodeLocationCanon;

        #endregion

        #region constructors

        [Inject]
        public CellScorer(
            IYieldEstimator yieldEstimator, IMapScorer mapScorer,
            IPossessionRelationship<IHexCell, IResourceNode> nodeLocationCanon
        ) {
            YieldEstimator    = yieldEstimator;
            MapScorer         = mapScorer;
            NodeLocationCanon = nodeLocationCanon;
        }

        #endregion

        #region instance methods

        #region from ICellScorer

        public float GetScoreOfCell(IHexCell cell) {
            float retval = MapScorer.GetScoreOfYield(YieldEstimator.GetYieldEstimateForCell(cell));

            foreach(var node in NodeLocationCanon.GetPossessionsOfOwner(cell)) {
                retval += MapScorer.GetScoreOfResourceNode(node);
            }

            return retval;
        }

        #endregion

        #endregion
        
    }

}
