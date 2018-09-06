using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;
using Assets.Simulation.Technology;

namespace Assets.Simulation.MapGeneration {

    public class CellScorer : ICellScorer {

        #region instance fields and properties

        private IYieldEstimator                                  YieldEstimator;
        private IMapScorer                                       MapScorer;
        private ITechCanon                                       TechCanon;
        private IPossessionRelationship<IHexCell, IResourceNode> NodeLocationCanon;

        #endregion

        #region constructors

        [Inject]
        public CellScorer(
            IYieldEstimator yieldEstimator, IMapScorer mapScorer, ITechCanon techCanon,
            IPossessionRelationship<IHexCell, IResourceNode> nodeLocationCanon
        ) {
            YieldEstimator    = yieldEstimator;
            MapScorer         = mapScorer;
            TechCanon         = techCanon;
            NodeLocationCanon = nodeLocationCanon;
        }

        #endregion

        #region instance methods

        #region from ICellScorer

        public float GetScoreOfCell(IHexCell cell) {
            var ancientTechs   = TechCanon.GetTechsOfEra(TechnologyEra.Ancient);
            var classicalTechs = TechCanon.GetTechsOfEra(TechnologyEra.Classical).Concat(ancientTechs);
            var medievalTechs  = TechCanon.GetTechsOfEra(TechnologyEra.Medieval) .Concat(classicalTechs);

            var ancientYield   = YieldEstimator.GetYieldEstimateForCell(cell, ancientTechs);
            var classicalYield = YieldEstimator.GetYieldEstimateForCell(cell, classicalTechs);
            var medievalYield  = YieldEstimator.GetYieldEstimateForCell(cell, medievalTechs);

            var resourceNode = NodeLocationCanon.GetPossessionsOfOwner(cell).FirstOrDefault();

            float ancientScore = MapScorer.GetScoreOfYield(ancientYield) +
                                 MapScorer.GetScoreOfResourceNode(resourceNode, ancientTechs);

            float classicalScore = MapScorer.GetScoreOfYield(classicalYield) +
                                   MapScorer.GetScoreOfResourceNode(resourceNode, classicalTechs);

            float medievalScore = MapScorer.GetScoreOfYield(medievalYield) +
                                  MapScorer.GetScoreOfResourceNode(resourceNode, medievalTechs);

            return (ancientScore + classicalScore + medievalScore) / 3f;
        }

        #endregion

        #endregion
        
    }

}
