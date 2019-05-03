﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine.Profiling;

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

            public float WorkInvested { get; set; }

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

            public void Destroy(bool immediateMode) {
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

        private Dictionary<IImprovementTemplate, HypotheticalImprovement> HypotheticalForTemplate =
            new Dictionary<IImprovementTemplate, HypotheticalImprovement>();

        private HypotheticalImprovement NullHypotheticalImprovement = new HypotheticalImprovement(null);




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
            var improvementModifications = new HashSet<IImprovementModificationData>(availableTechs.SelectMany(tech => tech.ImprovementYieldModifications));
            var visibleResources         = new HashSet<IResourceDefinition>         (TechCanon.GetDiscoveredResourcesFromTechs  (availableTechs));
            var availableImprovements    = new HashSet<IImprovementTemplate>        (TechCanon.GetAvailableImprovementsFromTechs(availableTechs));
            var availableBuildings       = new HashSet<IBuildingTemplate>           (TechCanon.GetAvailableBuildingsFromTechs   (availableTechs));

            return GetYieldEstimateForCell(
                cell,
                new CachedTechData() {
                    ImprovementModifications = improvementModifications,
                    VisibleResources         = visibleResources,
                    AvailableImprovements    = availableImprovements,
                    AvailableBuildings       = availableBuildings
                }
            );
        }


        public YieldSummary GetYieldEstimateForCell(IHexCell cell, CachedTechData techData) {
            var retval = YieldSummary.Empty;

            var nodeAtLocation = NodeLocationCanon.GetPossessionsOfOwner(cell).FirstOrDefault();

            var validImprovements = GetImprovementsValidFor(
                cell, nodeAtLocation, techData.VisibleResources, techData.AvailableImprovements
            );

            var yieldOfImprovements = validImprovements.Select(
                improvement => GetYieldEstimateForCellWithImprovement(
                    cell, nodeAtLocation, techData.VisibleResources, techData.ImprovementModifications,
                    techData.AvailableBuildingMods, improvement
                )
            );

            YieldSummary bestYield = YieldSummary.Empty;
            float bestScore = int.MinValue;

            if(yieldOfImprovements.Any()) {
                foreach(var yield in yieldOfImprovements) {
                    var score = MapScorer.GetScoreOfYield(yield);

                    if(score > bestScore) {
                        bestYield = yield;
                        bestScore = score;
                    }
                }
            }else {
                bestYield = GetYieldEstimateForCellWithImprovement(
                    cell, nodeAtLocation, techData.VisibleResources,
                    techData.ImprovementModifications, techData.AvailableBuildingMods, null
                );
            }

            return bestYield;
        }

        public YieldSummary GetYieldEstimateForResource(IResourceDefinition resource) {
            return resource.BonusYieldBase + resource.BonusYieldWhenImproved;
        }

        #endregion

        private YieldSummary GetYieldEstimateForCellWithImprovement(
            IHexCell cell, IResourceNode nodeAtLocation, IEnumerable<IResourceDefinition> visibleResources,
            IEnumerable<IImprovementModificationData> improvementModifications,
            IEnumerable<ICellYieldModificationData> buildingYieldMods,
            IImprovementTemplate improvement
        ) {
            var retval = YieldSummary.Empty;            

            if(nodeAtLocation != null) {
                HypotheticalImprovement hypothetical;

                if(improvement == null) {
                    hypothetical = NullHypotheticalImprovement;

                }else if(!HypotheticalForTemplate.TryGetValue(improvement, out hypothetical)) {
                    hypothetical = new HypotheticalImprovement(improvement);

                    HypotheticalForTemplate[improvement] = hypothetical;
                }

                retval += NodeYieldLogic.GetYieldFromNode(
                    nodeAtLocation, visibleResources, hypothetical
                );
            }

            bool clearVegetation = false;
            if(improvement != null) {
                retval += ImprovementYieldLogic.GetYieldOfImprovementTemplate(
                    improvement, nodeAtLocation, visibleResources, improvementModifications,
                    FreshWaterCanon.HasAccessToFreshWater(cell)
                );

                clearVegetation = improvement.ClearsVegetationWhenBuilt;
            }

            retval += InherentYieldLogic.GetInherentCellYield(cell, clearVegetation);

            retval += YieldFromBuildingsLogic.GetBonusCellYieldFromYieldModifications(cell, buildingYieldMods);

            return retval + OneScience;
        }

        private IEnumerable<IImprovementTemplate> GetImprovementsValidFor(
            IHexCell cell, IResourceNode nodeAtLocation,
            IEnumerable<IResourceDefinition> availableResources,
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
