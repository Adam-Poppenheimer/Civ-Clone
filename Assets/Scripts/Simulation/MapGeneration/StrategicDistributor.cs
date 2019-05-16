using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.Profiling;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapGeneration {

    public class StrategicDistributor : IStrategicDistributor {

        #region instance fields and properties

        private IResourceNodeFactory                        NodeFactory;
        private IResourceRestrictionLogic                   ResourceRestrictionCanon;
        private IStrategicCopiesLogic                       StrategicCopiesLogic;
        private IWeightedRandomSampler<IHexCell>            CellRandomSampler;
        private IWeightedRandomSampler<IResourceDefinition> ResourceRandomSampler;

        private IEnumerable<IResourceDefinition> StrategicResources;

        #endregion

        #region constructors

        [Inject]
        public StrategicDistributor(
            IResourceNodeFactory nodeFactory, IResourceRestrictionLogic resourceRestrictionCanon,
            IStrategicCopiesLogic strategicCopiesLogic, IWeightedRandomSampler<IHexCell> cellRandomSampler,
            [Inject(Id = "Available Resources")] IEnumerable<IResourceDefinition> availableResources,
            IWeightedRandomSampler<IResourceDefinition> resourceRandomSampler
        ) {
            NodeFactory              = nodeFactory;
            ResourceRestrictionCanon = resourceRestrictionCanon;
            StrategicCopiesLogic     = strategicCopiesLogic;
            CellRandomSampler        = cellRandomSampler;
            ResourceRandomSampler    = resourceRandomSampler;

            StrategicResources = availableResources.Where(resource => resource.Type == ResourceType.Strategic);
        }

        #endregion

        #region instance methods

        #region from IResourceDistributor

        public void DistributeStrategicsAcrossHomeland(HomelandData homelandData) {
            int nodesLeft  = Mathf.CeilToInt(homelandData.YieldAndResources.StrategicNodesPerCell  * homelandData.Cells.Count());
            int copiesLeft = Mathf.CeilToInt(homelandData.YieldAndResources.StrategicCopiesPerCell * homelandData.Cells.Count());

            var validStrategics = new List<IResourceDefinition>(StrategicResources);

            var resourceWeightsByRegion = new Dictionary<MapRegion, Dictionary<IResourceDefinition, int>>();

            foreach(var region in homelandData.AllRegions) {
                resourceWeightsByRegion[region] = homelandData.GetDataOfRegion(region).GetResourceWeights();
            }

            var regions = homelandData.AllRegions.ToList();
            int iterations = regions.Count * 10;

            while(nodesLeft > 0 && copiesLeft > 0 && iterations-- > 0) {
                if(regions.Count == 0) {
                    regions.AddRange(homelandData.AllRegions);
                }

                var region = regions.Random();
                regions.Remove(region);

                var resourceWeights = resourceWeightsByRegion[region];

                var strategic = ResourceRandomSampler.SampleElementsFromSet(
                    validStrategics, 1,
                    resource => resourceWeights.ContainsKey(resource) ? resourceWeights[resource] : 0
                ).FirstOrDefault();

                if(strategic == null) {
                    continue;
                }

                var location = CellRandomSampler.SampleElementsFromSet(
                    region.Cells, 1, cell => ResourceRestrictionCanon.GetPlacementWeightOnCell(strategic, cell)
                ).FirstOrDefault();

                if(location != null && NodeFactory.CanBuildNode(location, strategic)) {
                    int copies = StrategicCopiesLogic.GetWeightedRandomCopies();

                    copies = Math.Min(copies, copiesLeft);

                    NodeFactory.BuildNode(location, strategic, copies);

                    nodesLeft--;
                    copiesLeft -= copies;
                }
            }
        }

        #endregion

        #endregion

    }

}
