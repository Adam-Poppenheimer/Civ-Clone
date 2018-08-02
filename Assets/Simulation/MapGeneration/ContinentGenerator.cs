using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapGeneration {

    public class ContinentGenerator : IContinentGenerator {

        #region instance fields and properties

        private IHexGrid               Grid;
        private IMapGenerationConfig   Config;
        private IRegionGenerator       RegionGenerator;
        private IGridTraversalLogic    GridTraversalLogic;

        #endregion

        #region constructors

        [Inject]
        public ContinentGenerator(
            IHexGrid grid, IMapGenerationConfig config,
            IRegionGenerator startingLocationGenerator,
            IGridTraversalLogic gridTraversalLogic
        ) {
            Grid               = grid;
            Config             = config;
            RegionGenerator    = startingLocationGenerator;
            GridTraversalLogic = gridTraversalLogic;
        }

        #endregion

        #region instance methods

        #region from IContinentGenerator

         public void GenerateContinent(
             MapRegion continent, IContinentGenerationTemplate template,
             IEnumerable<IHexCell> oceanCells
        ) {
            var unassignedCells = new HashSet<IHexCell>(continent.Cells);

            var regions = SplitContinentIntoRegions(continent, template, unassignedCells);

            AssignOrphansToRegions(unassignedCells, regions);

            RegionGenerator.GenerateRegion(regions[0], template.StartingLocationTemplates.Random(), oceanCells);
            RegionGenerator.GenerateRegion(regions[1], template.BoundaryTemplates        .Random(), oceanCells);
            RegionGenerator.GenerateRegion(regions[2], template.StartingLocationTemplates.Random(), oceanCells);
        }

        #endregion

        private List<MapRegion> SplitContinentIntoRegions(
            MapRegion continent, IContinentGenerationTemplate template,
            HashSet<IHexCell> unassignedCells
        ) {
            if(continent.IsTallerThanWide()) {
                return SplitContinentIntoRegions_Vertical(continent, template, unassignedCells);
            }else {
                return SplitContinentIntoRegions_Horizontal(continent, template, unassignedCells);
            }
        }

        private List<MapRegion> SplitContinentIntoRegions_Vertical(
            MapRegion continent, IContinentGenerationTemplate template, HashSet<IHexCell> unassignedCells
        ) {
            var regions        = new List<MapRegion>();
            var regionCrawlers = new Dictionary<MapRegion, IEnumerator<IHexCell>>();

            var zBiasedWeightFunction = BuildAxisBiasedWeightFunction(100f, 1f);

            int xAverage = Mathf.RoundToInt((continent.XMin + continent.XMax) / 2f);
            int zSpan    = continent.ZMax - continent.ZMin;

            int totalRegionCount = 2 * template.StartingAreaCount - 1;

            for(int i = 0; i < totalRegionCount; i++) {
                var region = new MapRegion(Grid);                

                float ratioUpZSpan = (2 * i + 1) / (totalRegionCount * 2f);

                var seed = continent.GetClosestCellToCell(                    
                    Grid.GetCellAtOffset(xAverage, continent.ZMin + Mathf.RoundToInt(zSpan * ratioUpZSpan))
                );

                regionCrawlers[region] = GridTraversalLogic.GetCrawlingEnumerator(
                    seed, unassignedCells, region.Cells, zBiasedWeightFunction
                );

                regions.Add(region);
            }
            
            SplitCellsBetweenRegions(
                regions, regionCrawlers, unassignedCells
            );

            return regions;
        }

        private List<MapRegion> SplitContinentIntoRegions_Horizontal(
            MapRegion continent, IContinentGenerationTemplate template, HashSet<IHexCell> unassignedCells
        ) {
            var regions        = new List<MapRegion>();
            var regionCrawlers = new Dictionary<MapRegion, IEnumerator<IHexCell>>();

            var xBiasedWeightFunction = BuildAxisBiasedWeightFunction(1f, 100f);

            int xSpan    = continent.XMax - continent.XMin;
            int zAverage = Mathf.RoundToInt((continent.ZMin + continent.ZMax) / 2f);

            int totalRegionCount = 2 * template.StartingAreaCount - 1;

            for(int i = 0; i < totalRegionCount; i++) {
                var region = new MapRegion(Grid);                

                float ratioAcrossXSpan = (2 * i + 1) / (totalRegionCount * 2f);

                var seed = continent.GetClosestCellToCell(
                    Grid.GetCellAtOffset(continent.XMin + Mathf.RoundToInt(xSpan * ratioAcrossXSpan), zAverage)
                );

                regionCrawlers[region] = GridTraversalLogic.GetCrawlingEnumerator(
                    seed, unassignedCells, region.Cells, xBiasedWeightFunction
                );

                regions.Add(region);
            }
            
            SplitCellsBetweenRegions(
                regions, regionCrawlers, unassignedCells
            );

            return regions;
        }

        private void SplitCellsBetweenRegions(
            List<MapRegion> regions, Dictionary<MapRegion, IEnumerator<IHexCell>> crawlers,
            HashSet<IHexCell> unassignedCells
        ) {
            int regionCellTarget = Mathf.CeilToInt(unassignedCells.Count / (float)regions.Count);

            int iterations = unassignedCells.Count * 10;
            while(unassignedCells.Count > 0 && iterations-- > 0) {
                foreach(var region in regions) {
                    var crawler = crawlers[region];

                    if(region.Cells.Count < regionCellTarget && crawler.MoveNext()) {
                        var newCell = crawler.Current;

                        region.AddCell(newCell);
                        unassignedCells.Remove(newCell);
                    }
                }
            }
        }

        private int RegionWeightFunction(IHexCell cell, MapRegion region) {
            int distanceFromSeed = Grid.GetDistance(cell, region.Seed);
            int jitter = UnityEngine.Random.value < Config.JitterProbability ? 1 : 0;

            return distanceFromSeed + jitter;
        }

        private CrawlingWeightFunction BuildAxisBiasedWeightFunction(float xBias, float zBias) {
            return delegate(IHexCell cell, IHexCell seed, IEnumerable<IHexCell> acceptedCells) {
                int cellOffsetX = HexCoordinates.ToOffsetCoordinateX(cell.Coordinates);
                int cellOffsetZ = HexCoordinates.ToOffsetCoordinateZ(cell.Coordinates);

                int seedOffsetX = HexCoordinates.ToOffsetCoordinateX(seed.Coordinates);
                int seedOffsetZ = HexCoordinates.ToOffsetCoordinateZ(seed.Coordinates);

                int xDistance = Math.Abs(cellOffsetX - seedOffsetX);
                int zDistance = Math.Abs(cellOffsetZ - seedOffsetZ);

                return Mathf.RoundToInt(xDistance * xBias + zDistance * zBias);
            };
        }

        private void AssignOrphansToRegions(HashSet<IHexCell> unassignedCells, List<MapRegion> regions) {
            foreach(var orphan in unassignedCells) {
                var mostAppropriateRegion = regions.Aggregate(delegate(MapRegion currentRegion, MapRegion nextRegion) {
                    var neighbors = Grid.GetNeighbors(orphan);

                    int neighborsInCurrent = neighbors.Where(neighbor => currentRegion.Cells.Contains(neighbor)).Count();
                    int neighborsInNext    = neighbors.Where(neighbor => nextRegion   .Cells.Contains(neighbor)).Count();

                    return neighborsInCurrent >= neighborsInNext ? currentRegion : nextRegion;
                });

                mostAppropriateRegion.AddCell(orphan);
            }
        }

        #endregion
       
    }

}
