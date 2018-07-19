using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapGeneration {

    public class SubdividingContinentGenerator : IContinentGenerator {

        #region instance fields and properties

        private ICellModificationLogic ModLogic;
        private IHexGrid               Grid;
        private IMapGenerationConfig   Config;
        private IRegionGenerator       RegionGenerator;
        private IGridTraversalLogic    GridTraversalLogic;

        #endregion

        #region constructors

        [Inject]
        public SubdividingContinentGenerator(
            ICellModificationLogic modLogic, IHexGrid grid, IMapGenerationConfig config,
            IRegionGenerator startingLocationGenerator, IGridTraversalLogic gridTraversalLogic
        ) {
            ModLogic           = modLogic;
            Grid               = grid;
            Config             = config;
            RegionGenerator    = startingLocationGenerator;
            GridTraversalLogic = gridTraversalLogic;
        }

        #endregion

        #region instance methods

        #region from IContinentGenerator

         public void GenerateContinent(MapRegion continent) {
            foreach(var cell in continent.Cells) {
                ModLogic.ChangeTerrainOfCell(cell, CellTerrain.Grassland);
            }

            var unassignedCells = new HashSet<IHexCell>(continent.Cells);

            var regions = SplitContinentIntoRegions(continent, unassignedCells);

            AssignOrphansToRegions(unassignedCells, regions);

            RegionGenerator.GenerateRegion(regions[0], Config.StartingLocationTemplates.Random());
            RegionGenerator.GenerateRegion(regions[1], Config.BoundaryTemplates        .Random());
            RegionGenerator.GenerateRegion(regions[2], Config.StartingLocationTemplates.Random());

            foreach(var cell in regions[1].Cells) {
                ModLogic.ChangeTerrainOfCell(cell, CellTerrain.Desert);
            }
        }

        #endregion

        private List<MapRegion> SplitContinentIntoRegions(MapRegion continent, HashSet<IHexCell> unassignedCells) {
            if(continent.IsTallerThanWide()) {
                return SplitContinentIntoRegions_Vertical(continent, unassignedCells);
            }else {
                return SplitContinentIntoRegions_Horizontal(continent, unassignedCells);
            }
        }

        private List<MapRegion> SplitContinentIntoRegions_Vertical(
            MapRegion continent, HashSet<IHexCell> unassignedCells
        ) {
            var topRegion    = new MapRegion(Grid);
            var middleRegion = new MapRegion(Grid);
            var bottomRegion = new MapRegion(Grid);

            var zBiasedWeightFunction = BuildAxisBiasedWeightFunction(100f, 1f);

            int xAverage = Mathf.RoundToInt((continent.XMin + continent.XMax) / 2f);
            int zSpan    = continent.ZMax - continent.ZMin;

            IHexCell topSeed    = continent.GetClosestCellToCell(Grid.GetCellAtOffset(xAverage, continent.ZMin + Mathf.RoundToInt(zSpan * 5f / 6f)));
            IHexCell middleSeed = continent.GetClosestCellToCell(Grid.GetCellAtOffset(xAverage, continent.ZMin + Mathf.RoundToInt(zSpan * 3f / 6f)));
            IHexCell bottomSeed = continent.GetClosestCellToCell(Grid.GetCellAtOffset(xAverage, continent.ZMin + Mathf.RoundToInt(zSpan * 1f / 6f)));

            var topCrawler = GridTraversalLogic.GetCrawlingEnumerator(
                topSeed, unassignedCells, zBiasedWeightFunction
            );

            var middleCrawler = GridTraversalLogic.GetCrawlingEnumerator(
                middleSeed, unassignedCells, zBiasedWeightFunction
            );

            var bottomCrawler = GridTraversalLogic.GetCrawlingEnumerator(
                bottomSeed, unassignedCells, zBiasedWeightFunction
            );
            
            SplitCellsBetweenRegions(
                topRegion, topCrawler, middleRegion, middleCrawler,
                bottomRegion, bottomCrawler, unassignedCells
            );

            return new List<MapRegion>() { topRegion, middleRegion, bottomRegion };
        }

        private List<MapRegion> SplitContinentIntoRegions_Horizontal(
            MapRegion continent, HashSet<IHexCell> unassignedCells
        ) {
            var leftRegion   = new MapRegion(Grid);
            var middleRegion = new MapRegion(Grid);
            var rightRegion  = new MapRegion(Grid);

            var xBiasedWeightFunction = BuildAxisBiasedWeightFunction(1f, 100f);

            int xSpan    = continent.XMax - continent.XMin;
            int zAverage = Mathf.RoundToInt((continent.ZMin + continent.ZMax) / 2f);

            IHexCell leftSeed   = continent.GetClosestCellToCell(Grid.GetCellAtOffset(continent.XMin + Mathf.RoundToInt(xSpan * 1f / 6f), zAverage));
            IHexCell middleSeed = continent.GetClosestCellToCell(Grid.GetCellAtOffset(continent.XMin + Mathf.RoundToInt(xSpan * 3f / 6f), zAverage));
            IHexCell rightSeed  = continent.GetClosestCellToCell(Grid.GetCellAtOffset(continent.XMin + Mathf.RoundToInt(xSpan * 5f / 6f), zAverage));

            var leftCrawler = GridTraversalLogic.GetCrawlingEnumerator(
                leftSeed, unassignedCells, xBiasedWeightFunction
            );

            var middleCrawler = GridTraversalLogic.GetCrawlingEnumerator(
                middleSeed, unassignedCells, xBiasedWeightFunction
            );

            var rightCrawler = GridTraversalLogic.GetCrawlingEnumerator(
                rightSeed, unassignedCells, xBiasedWeightFunction
            );

            SplitCellsBetweenRegions(
                leftRegion, leftCrawler, middleRegion, middleCrawler,
                rightRegion, rightCrawler, unassignedCells
            );

            return new List<MapRegion>() { leftRegion, middleRegion, rightRegion };
        }

        private void SplitCellsBetweenRegions(
            MapRegion regionOne,   IEnumerator<IHexCell> regionOneCrawler,
            MapRegion regionTwo,   IEnumerator<IHexCell> regionTwoCrawler,
            MapRegion regionThree, IEnumerator<IHexCell> regionThreeCrawler,
            HashSet<IHexCell> unassignedCells
        ) {
            int regionCellTarget = Mathf.CeilToInt(unassignedCells.Count / 3f);

            int firstCellCount = 0, secondCellCount = 0, thirdCellCount = 0;

            int iterations = unassignedCells.Count * 10;
            while(unassignedCells.Count > 0 && iterations-- > 0) {

                if(firstCellCount < regionCellTarget && regionOneCrawler.MoveNext()) {
                    var newCell = regionOneCrawler.Current;

                    regionOne.AddCell(newCell);
                    unassignedCells.Remove(newCell);

                    firstCellCount++;
                }

                if(secondCellCount < regionCellTarget && regionTwoCrawler.MoveNext()) {
                    var newCell = regionTwoCrawler.Current;

                    regionTwo.AddCell(newCell);
                    unassignedCells.Remove(newCell);

                    secondCellCount++;
                }

                if(thirdCellCount < regionCellTarget && regionThreeCrawler.MoveNext()) {
                    var newCell = regionThreeCrawler.Current;

                    regionThree.AddCell(newCell);
                    unassignedCells.Remove(newCell);

                    thirdCellCount++;
                }
            }
        }

        private int RegionWeightFunction(IHexCell cell, MapRegion region) {
            int distanceFromSeed = Grid.GetDistance(cell, region.Seed);
            int jitter = UnityEngine.Random.value < Config.JitterProbability ? 1 : 0;

            return distanceFromSeed + jitter;
        }

        private Func<IHexCell, IHexCell, int> BuildAxisBiasedWeightFunction(float xBias, float zBias) {
            return delegate(IHexCell cell, IHexCell seed) {
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
