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
        private IFreshWaterLogic                                 FreshWaterCanon;
        private ICellYieldFromBuildingsLogic                     YieldFromBuildingsLogic;
        private ITechCanon                                       TechCanon;
        private IImprovementValidityLogic                        ImprovementValidityLogic;
        private IMapScorer                                       MapScorer;

        #endregion

        #region constructors

        [Inject]
        public YieldEstimator(
            IInherentCellYieldLogic                          inherentYieldLogic,
            IPossessionRelationship<IHexCell, IResourceNode> nodeLocationCanon,
            IResourceNodeYieldLogic                          nodeYieldLogic,
            IImprovementYieldLogic                           improvementYieldLogic,
            IFreshWaterLogic                                 freshWaterCanon,
            ICellYieldFromBuildingsLogic                     yieldFromBuildingsLogic,
            ITechCanon                                       techCanon,
            IImprovementValidityLogic                        improvementValidityLogic,
            IMapScorer                                       mapScorer
        ) {
            InherentYieldLogic       = inherentYieldLogic;
            NodeLocationCanon        = nodeLocationCanon;
            NodeYieldLogic           = nodeYieldLogic;
            ImprovementYieldLogic    = improvementYieldLogic;
            FreshWaterCanon          = freshWaterCanon;
            YieldFromBuildingsLogic  = yieldFromBuildingsLogic;
            TechCanon                = techCanon;
            ImprovementValidityLogic = improvementValidityLogic;
            MapScorer                = mapScorer;
        }

        #endregion

        #region instance methods

        #region from CellYieldEstimator

        public YieldSummary GetYieldEstimateForCell(
            IHexCell cell, IEnumerable<ITechDefinition> availableTechs
        ) {
            var retval = YieldSummary.Empty;

            var visibleResources      = TechCanon.GetDiscoveredResourcesFromTechs     (availableTechs).ToList();
            var availableImprovements = TechCanon.GetAvailableImprovementsFromTechs(availableTechs).ToList();
            var availableBuildings    = TechCanon.GetAvailableBuildingsFromTechs   (availableTechs).ToList();

            var nodeAtLocation = NodeLocationCanon.GetPossessionsOfOwner(cell).FirstOrDefault();

            var validImprovements = GetImprovementsValidFor(
                cell, nodeAtLocation, visibleResources, availableImprovements
            );

            var yieldOfImprovements = validImprovements.Select(
                improvement => GetYieldEstimateForCellWithImprovement(
                    cell, nodeAtLocation, visibleResources, availableTechs,
                    availableBuildings, improvement
                )
            );

            YieldSummary bestYield = YieldSummary.Empty;
            float bestScore = int.MinValue;

            foreach(var yield in yieldOfImprovements) {
                var score = MapScorer.GetScoreOfYield(yield);

                if(score > bestScore) {
                    bestYield = yield;
                    bestScore = score;
                }
            }

            return bestYield;
        }

        public YieldSummary GetYieldEstimateForResource(IResourceDefinition resource) {
            return resource.BonusYieldBase + resource.BonusYieldWhenImproved;
        }

        #endregion

        private YieldSummary GetYieldEstimateForCellWithImprovement(
            IHexCell cell, IResourceNode nodeAtLocation, IEnumerable<IResourceDefinition> visibleResources,
            IEnumerable<ITechDefinition> availableTechs, IEnumerable<IBuildingTemplate> availableBuildings,
            IImprovementTemplate improvement
        ) {
            var retval = YieldSummary.Empty;

            if(nodeAtLocation != null) {
                retval += NodeYieldLogic.GetYieldFromNode(
                    nodeAtLocation, visibleResources, new HypotheticalImprovement(improvement)
                );
            }

            bool clearVegetation = false;
            if(improvement != null) {
                retval += ImprovementYieldLogic.GetYieldOfImprovementTemplate(
                    improvement, nodeAtLocation, visibleResources, availableTechs,
                    FreshWaterCanon.HasAccessToFreshWater(cell)
                );

                clearVegetation = improvement.ClearsVegetationWhenBuilt;
            }

            retval += InherentYieldLogic.GetInherentCellYield(cell, clearVegetation);

            retval += YieldFromBuildingsLogic.GetBonusCellYieldFromBuildings(cell, availableBuildings);

            return retval + OneScience;
        }

        private IEnumerable<IImprovementTemplate> GetImprovementsValidFor(
            IHexCell cell, IResourceNode nodeAtLocation, IEnumerable<IResourceDefinition> availableResources,
            IEnumerable<IImprovementTemplate> availableImprovements
        ) {
            if( nodeAtLocation != null &&
                nodeAtLocation.Resource.Extractor != null &&
                availableResources.Contains(nodeAtLocation.Resource) &&
                availableImprovements.Contains(nodeAtLocation.Resource.Extractor)
            ) {
                return new List<IImprovementTemplate>() { nodeAtLocation.Resource.Extractor };

            }else {
                var retval = availableImprovements.Where(
                    improvement => ImprovementValidityLogic.IsTemplateValidForCell(improvement, cell, true)
                ).ToList();

                retval.Add(null);

                return retval;
            }
        }

        #endregion

    }

}
