using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public class OceanGenerator : IOceanGenerator {

        #region instance fields and properties

        private ICellModificationLogic  ModLogic;
        private IRegionGenerator        RegionGenerator;
        private ITemplateSelectionLogic TemplateSelectionLogic;
        private IHexGrid                Grid;

        private List<IBalanceStrategy> AvailableBalanceStrategies;

        #endregion

        #region constructors

        [Inject]
        public OceanGenerator(
            ICellModificationLogic modLogic, IRegionGenerator regionGenerator,
            ITemplateSelectionLogic templateSelectionLogic, IHexGrid grid,
            List<IBalanceStrategy> availableBalanceStrategies
        ) {
            ModLogic                   = modLogic;
            RegionGenerator            = regionGenerator;
            TemplateSelectionLogic     = templateSelectionLogic;
            Grid                       = grid;
            AvailableBalanceStrategies = availableBalanceStrategies;
        }

        #endregion

        #region instance methods

        #region from IOceanGenerator

        public OceanData GetOceanData(
            IEnumerable<MapSection> oceanSections, IOceanTemplate oceanTemplate,
            IMapTemplate mapTemplate, GridPartition partition
        ) {
            var shallowOceanSections = oceanSections.Where(
                section => partition.GetNeighbors(section).Any(neighbor => !oceanSections.Contains(neighbor))
            ).ToList();

            var midOceanSections = oceanSections.Except(shallowOceanSections).Where(
                section => partition.GetNeighbors(section).Any(neighbor => shallowOceanSections.Contains(neighbor))
            ).ToList();

            var deepOceanSections = oceanSections.Except(shallowOceanSections).Except(midOceanSections);

            List<MapRegion> emptyOceanRegions;
            List<MapRegion> archipelagoRegions;
            List<RegionData> archipelagoRegionData;

            CarveArchipelagoesFromOcean(
                shallowOceanSections, midOceanSections, deepOceanSections, oceanTemplate, partition,
                mapTemplate, out emptyOceanRegions, out archipelagoRegions, out archipelagoRegionData
            );

            return new OceanData(emptyOceanRegions, archipelagoRegions, archipelagoRegionData);
        }

        public void GenerateTopologyAndEcology(OceanData oceanData) {
            foreach(var region in oceanData.EmptyOceanRegions) {
                foreach(var cell in region.Cells) {
                    ModLogic.ChangeTerrainOfCell(cell, CellTerrain.DeepWater);
                }
            }

            foreach(var region in oceanData.ArchipelagoRegions) {
                RegionData regionData = oceanData.GetRegionData(region);

                RegionGenerator.GenerateTopology  (region, regionData.Topology);
                RegionGenerator.PaintTerrain      (region, regionData.Biome);
                RegionGenerator.AssignFloodPlains (region.LandCells);
            }
        }

        public void DistributeYieldAndResources(OceanData oceanData) {
            Debug.Log("Yield and resource distribution for oceans is currently unimplemented");
        }

        #endregion

        private void CarveArchipelagoesFromOcean(
            IEnumerable<MapSection> shallowOceanSections, List<MapSection> midOceanSections,
            IEnumerable<MapSection> deepOceanSections, IOceanTemplate oceanTemplate, GridPartition partition,
            IMapTemplate mapTemplate, out List<MapRegion> emptyOceanRegions,
            out List<MapRegion> archipelagoRegions, out List<RegionData> archipelagoRegionData
        ) {
            int landSectionCount = Mathf.RoundToInt(oceanTemplate.DeepOceanLandPercentage * 0.01f * deepOceanSections.Count());

            var deepOceanLand = WeightedRandomSampler<MapSection>.SampleElementsFromSet(
                deepOceanSections, landSectionCount, GetArchipelagoWeightFunction(mapTemplate)
            ).ToList();

            var deepOceanWater = deepOceanSections.Except(deepOceanLand).ToList();

            List<MapSection> emptyDeepOcean;

            GetArchipelagoesFromLandAndWater(
                deepOceanLand, deepOceanWater, midOceanSections, oceanTemplate,
                partition, out archipelagoRegions, out emptyDeepOcean
            );

            var emptyOceanCells = shallowOceanSections.Concat(midOceanSections).Concat(emptyDeepOcean);

            var emptyOcean = new MapRegion(
                new List<IHexCell>(),
                emptyOceanCells.SelectMany(section => section.Cells).ToList()
            );

            emptyOceanRegions = new List<MapRegion>() { emptyOcean };

            archipelagoRegionData = archipelagoRegions.Select(region => new RegionData(
                TemplateSelectionLogic.GetBiomeForLandRegion(region, mapTemplate),
                TemplateSelectionLogic.GetTopologyForLandRegion(region, mapTemplate),
                AvailableBalanceStrategies
            )).ToList();
        }

        private void GetArchipelagoesFromLandAndWater(
            List<MapSection> landSections, List<MapSection> deepWaterSections, List<MapSection> midOceanSections,
            IOceanTemplate template, GridPartition partition, out List<MapRegion> archipelagoRegions,
            out List<MapSection> emptyDeepOcean
        ) {
            archipelagoRegions = new List<MapRegion>();

            while(landSections.Any()) {
                var contiguousLand = GetContiguousLand(landSections.Last(), landSections, partition);

                foreach(var land in contiguousLand) {
                    landSections.Remove(land);
                }

                var coastSections = contiguousLand.SelectMany(section => partition.GetNeighbors(section))
                                                  .Where(neighbor => deepWaterSections.Contains(neighbor) || midOceanSections.Contains(neighbor))
                                                  .Distinct().ToList();

                foreach(var coast in coastSections) {
                    deepWaterSections.Remove(coast);
                    midOceanSections .Remove(coast);
                }

                archipelagoRegions.Add(new MapRegion(
                    contiguousLand.SelectMany(section => section.Cells).ToList(),
                    coastSections .SelectMany(section => section.Cells).ToList()
                ));
            }

            emptyDeepOcean = deepWaterSections;            
        }

        private IEnumerable<MapSection> GetContiguousLand(
            MapSection seed, List<MapSection> landSections, GridPartition partition
        ) {
            var uncheckedSections = new List<MapSection>() { seed };

            var contiguousSection = new List<MapSection>();

            while(uncheckedSections.Any()) {
                var uncheckedSection = uncheckedSections.Last();
                uncheckedSections.Remove(uncheckedSection);

                contiguousSection.Add(uncheckedSection);

                foreach(var neighbor in partition.GetNeighbors(uncheckedSection).Intersect(landSections).Except(contiguousSection)) {
                    uncheckedSections.Add(neighbor);
                }
            }

            return contiguousSection;
        }

        private bool IsWithinRange(
            MapSection section, float xCoordinateMin, float xCoordinateMax,
            float zCoordinateMin, float zCoordinateMax
        ) {
            var centroidCoordX = HexCoordinates.ToOffsetCoordinateX(section.CentroidCell.Coordinates);
            var centroidCoordZ = HexCoordinates.ToOffsetCoordinateZ(section.CentroidCell.Coordinates);

            return centroidCoordX >= xCoordinateMin
                && centroidCoordX <  xCoordinateMax
                && centroidCoordZ >= zCoordinateMin
                && centroidCoordZ <  zCoordinateMax;
        }

        private Func<MapSection, int> GetArchipelagoWeightFunction(IMapTemplate template) {
            return delegate(MapSection section) {
                int centroidCellX = HexCoordinates.ToOffsetCoordinateX(section.CentroidCell.Coordinates);
                int centroidCellZ = HexCoordinates.ToOffsetCoordinateZ(section.CentroidCell.Coordinates);

                if( centroidCellX                   < template.HardMapBorderX ||
                    Grid.CellCountX - centroidCellX < template.HardMapBorderX ||
                    centroidCellZ                   < template.HardMapBorderZ ||
                    Grid.CellCountZ - centroidCellZ < template.HardMapBorderZ
                ) {
                    return 0;
                }else {
                    return 1;
                }
            };
        }

        #endregion

    }

}
