using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;
using Assets.Simulation.Improvements;
using Assets.Simulation.Technology;
using Assets.Simulation.Cities.Buildings;
using UnityEngine;

namespace Assets.Simulation.MapGeneration {

    public class YieldEstimator : IYieldEstimator {

        #region internal types

        private class HypotheticalImprovement : IImprovement {

            #region instance fields and properties

            public bool IsConstructed { get { return true;  } }
            public bool IsPillaged    { get { return false; } }

            public bool IsReadyToConstruct { get; set; }

            public IImprovementTemplate Template { get; set; }

            public Transform transform { get; set; }

            public int WorkInvested { get; set; }

            #endregion

            #region constructors

            public HypotheticalImprovement(IImprovementTemplate template) {
                Template = template;
            }

            #endregion

            #region instance methods

            public void Construct() {
                throw new NotImplementedException();
            }

            public void Destroy() {
                throw new NotImplementedException();
            }

            public void Pillage() {
                throw new NotImplementedException();
            }

            #endregion

        }

        #endregion

        #region static fields and properties

        private static YieldSummary OneScience = new YieldSummary(science: 1);

        #endregion

        #region instance fields and properties

        private IInherentCellYieldLogic                          InherentYieldLogic;
        private IPossessionRelationship<IHexCell, IResourceNode> NodeLocationCanon;
        private IResourceNodeYieldLogic                          NodeYieldLogic;
        private IImprovementYieldLogic                           ImprovementYieldLogic;
        private IFreshWaterCanon                                 FreshWaterCanon;
        private ICellYieldFromBuildingsLogic                     YieldFromBuildingsLogic;
        private IMapScorer                                     YieldScorer;
        private IImprovementValidityLogic                        ImprovementValidityLogic;

        private IEnumerable<IResourceDefinition>  AvailableResources;
        private IEnumerable<ITechDefinition>      AvailableTechs;
        private IEnumerable<IBuildingTemplate>    AvailableBuildings;
        private IEnumerable<IImprovementTemplate> AvailableImprovements;

        #endregion

        #region constructors

        [Inject]
        public YieldEstimator(
            IInherentCellYieldLogic                          inherentYieldLogic,
            IPossessionRelationship<IHexCell, IResourceNode> nodeLocationCanon,
            IResourceNodeYieldLogic                          nodeYieldLogic,
            IImprovementYieldLogic                           improvementYieldLogic,
            IFreshWaterCanon                                 freshWaterCanon,
            ICellYieldFromBuildingsLogic                     yieldFromBuildingsLogic,
            IMapScorer                                       mapScorer,
            IImprovementValidityLogic                        improvementValidityLogic,

            [Inject(Id = "Available Resources")]             IEnumerable<IResourceDefinition>  availableResources,
                                                             ITechCanon                        techCanon,
                                                             List<IBuildingTemplate>           availableBuildings,
            [Inject(Id = "Available Improvement Templates")] IEnumerable<IImprovementTemplate> availableImprovements
        ) {
            InherentYieldLogic       = inherentYieldLogic;
            NodeLocationCanon        = nodeLocationCanon;
            NodeYieldLogic           = nodeYieldLogic;
            ImprovementYieldLogic    = improvementYieldLogic;
            FreshWaterCanon          = freshWaterCanon;
            YieldFromBuildingsLogic  = yieldFromBuildingsLogic;
            YieldScorer              = mapScorer;
            ImprovementValidityLogic = improvementValidityLogic;

            AvailableResources    = availableResources;
            AvailableTechs        = techCanon.AvailableTechs;
            AvailableBuildings    = availableBuildings;
            AvailableImprovements = availableImprovements;
        }

        #endregion

        #region instance methods

        #region from CellYieldEstimator

        public YieldSummary GetYieldEstimateForCell(IHexCell cell) {
            var retval = YieldSummary.Empty;

            retval += InherentYieldLogic.GetInherentCellYield(cell);

            var nodeAtLocation = NodeLocationCanon.GetPossessionsOfOwner(cell).FirstOrDefault();

            var expectedImprovement = GetExpectedImprovementForCell(cell, nodeAtLocation);

            if(nodeAtLocation != null) {
                retval += NodeYieldLogic.GetYieldFromNode(
                    nodeAtLocation, AvailableResources, new HypotheticalImprovement(expectedImprovement)
                );
            }
            
            if(expectedImprovement != null) {
                retval += ImprovementYieldLogic.GetYieldOfImprovementTemplate(
                    expectedImprovement, nodeAtLocation, AvailableResources, AvailableTechs,
                    FreshWaterCanon.HasAccessToFreshWater(cell)
                );
            }

            retval += YieldFromBuildingsLogic.GetBonusCellYieldFromBuildings(cell, AvailableBuildings);

            return retval + OneScience;               
        }

        public YieldSummary GetYieldEstimateForResource(IResourceDefinition resource) {
            return resource.BonusYieldBase + resource.BonusYieldWhenImproved;
        }

        #endregion

        private IImprovementTemplate GetExpectedImprovementForCell(IHexCell cell, IResourceNode nodeAtLocation) {
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

                float yieldScore = YieldScorer.GetScoreOfYield(improvementYield);

                if(yieldScore > bestScore) {
                    bestTemplate = improvementTemplate;
                    bestScore = yieldScore;
                }
            }
            
            return bestTemplate;
        }

        #endregion

    }

}
