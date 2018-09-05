using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;
using Assets.Simulation.Improvements;
using Assets.Simulation.Technology;

namespace Assets.Simulation.MapGeneration {

    public class ImprovementEstimator : IImprovementEstimator {

        #region instance fields and properties

        private IFreshWaterCanon          FreshWaterCanon;
        private IImprovementValidityLogic ImprovementValidityLogic;
        private IImprovementYieldLogic    ImprovementYieldLogic;
        private IMapScorer                MapScorer;


        private IEnumerable<IResourceDefinition>  AvailableResources;
        private IEnumerable<ITechDefinition>      AvailableTechs;
        private IEnumerable<IImprovementTemplate> AvailableImprovements;

        #endregion

        #region constructors

        [Inject]
        public ImprovementEstimator(
            IFreshWaterCanon freshWaterCanon, IImprovementValidityLogic improvementValidityLogic,
            IImprovementYieldLogic improvementYieldLogic, IMapScorer mapScorer, ITechCanon techCanon,
            [Inject(Id = "Available Resources")]             IEnumerable<IResourceDefinition>  availableResources,                                                             
            [Inject(Id = "Available Improvement Templates")] IEnumerable<IImprovementTemplate> availableImprovements
        ) {
            FreshWaterCanon          = freshWaterCanon;
            ImprovementValidityLogic = improvementValidityLogic;
            ImprovementYieldLogic    = improvementYieldLogic;
            MapScorer                = mapScorer;

            AvailableResources    = availableResources;
            AvailableTechs        = techCanon.AvailableTechs;
            AvailableImprovements = availableImprovements;
        }

        #endregion

        #region instance methods

        #region from IImprovementEstimator

        public IImprovementTemplate GetExpectedImprovementForCell(IHexCell cell, IResourceNode nodeAtLocation) {
            if(nodeAtLocation != null && nodeAtLocation.Resource.Extractor != null) {
                return nodeAtLocation.Resource.Extractor;
            }

            IImprovementTemplate bestTemplate = null;
            float bestScore = int.MinValue;

            foreach(var improvementTemplate in AvailableImprovements) {
                if(!ImprovementValidityLogic.IsTemplateValidForCell(improvementTemplate, cell, true)) {
                    continue;
                }

                YieldSummary improvementYield = ImprovementYieldLogic.GetYieldOfImprovementTemplate(
                    improvementTemplate, nodeAtLocation, AvailableResources, AvailableTechs,
                    FreshWaterCanon.HasAccessToFreshWater(cell)
                );

                float yieldScore = MapScorer.GetScoreOfYield(improvementYield);

                if(yieldScore > bestScore) {
                    bestTemplate = improvementTemplate;
                    bestScore = yieldScore;
                }
            }
            
            return bestTemplate;
        }

        #endregion

        #endregion

    }

}
