using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapGeneration {

    public class RegionGenerator : IRegionGenerator {

        #region instance fields and properties

        private ICellModificationLogic ModLogic;
        private IHexGrid               Grid;
        private IGridTraversalLogic    GridTraversalLogic;
        private IRiverCanon            RiverCanon;
        private IRiverGenerator        RiverGenerator;
        private IResourceDistributor   ResourceDistributor;
        private IRegionBalancer        RegionBalancer;
        private IMapGenerationConfig   Config;

        #endregion

        #region constructors

        [Inject]
        public RegionGenerator(
            ICellModificationLogic modLogic, IHexGrid grid,
            IGridTraversalLogic gridTraversalLogic, IRiverCanon riverCanon,
            IRiverGenerator riverGenerator, IResourceDistributor resourceDistributor,
            IRegionBalancer regionBalancer, IMapGenerationConfig config
        ) {
            ModLogic            = modLogic;
            Grid                = grid;
            GridTraversalLogic  = gridTraversalLogic;
            RiverCanon          = riverCanon;
            RiverGenerator      = riverGenerator;
            ResourceDistributor = resourceDistributor;
            RegionBalancer      = regionBalancer;
            Config              = config;
        }

        #endregion

        #region instance methods

        #region from IStartingLocationGenerator

        public void GenerateTopologyAndEcology(
            MapRegion region, IRegionTemplate template
        ) {
            GenerateTopology(region, template);
            PaintTerrain(region, template);

            RiverGenerator.CreateRiversForRegion(region.LandCells, template, region.WaterCells);

            AssignFloodPlains(region.LandCells);

            PaintVegetation(region.LandCells, template);
        }

        public void DistributeYieldAndResources(
            MapRegion region, IRegionTemplate template
        ) {
            ResourceDistributor.DistributeLuxuryResourcesAcrossRegion   (region, template);
            ResourceDistributor.DistributeStrategicResourcesAcrossRegion(region, template);

            RegionBalancer.BalanceRegionYields(region, template);
        }

        #endregion

        private void GenerateTopology(MapRegion region, IRegionTemplate template) {
            var landCells = region.LandCells;

            int desiredMountainCount = Mathf.RoundToInt(template.MountainsPercentage * landCells.Count() * 0.01f);
            int desiredHillsCount    = Mathf.RoundToInt(template.HillsPercentage     * landCells.Count() * 0.01f);

            var elevatedCells = WeightedRandomSampler<IHexCell>.SampleElementsFromSet(
                landCells, desiredHillsCount + desiredMountainCount,
                HillsStartingWeightFunction, HillsDynamicWeightFunction, cell => Grid.GetNeighbors(cell)
            );

            foreach(var cell in elevatedCells) {
                ModLogic.ChangeShapeOfCell(cell, CellShape.Hills);
            }

            var mountainousCells = WeightedRandomSampler<IHexCell>.SampleElementsFromSet(
                elevatedCells, desiredMountainCount, MountainWeightFunction
            );

            foreach(var cell in mountainousCells) {
                ModLogic.ChangeShapeOfCell(cell, CellShape.Mountains);
            }
        }

        private void PaintTerrain(
            MapRegion region, IRegionTemplate template
        ) {
            var landTerrains = EnumUtil.GetValues<CellTerrain>().Where(terrain => !terrain.IsWater());

            var dataOfTerrains  = new Dictionary<CellTerrain, TerrainData>();
            var terrainCounts   = new Dictionary<CellTerrain, int>();
            var terrainCrawlers = new Dictionary<CellTerrain, List<IEnumerator<IHexCell>>>();
            var cellsOfTerrain  = new Dictionary<CellTerrain, List<IHexCell>>();

            var unassignedLandCells = new HashSet<IHexCell>(region.LandCells);
            var seeds = new List<IHexCell>();

            foreach(var terrain in landTerrains) {
                var terrainData = template.GetTerrainData(terrain);
                dataOfTerrains[terrain] = terrainData;

                var terrainCount = Mathf.RoundToInt(unassignedLandCells.Count() * terrainData.Percentage * 0.01f);

                var acceptedCells = new List<IHexCell>();

                var crawlers = new List<IEnumerator<IHexCell>>();

                for(int i = 0; i < terrainData.SeedCount; i++) {
                    var seedCandidates = unassignedLandCells.Where(
                        cell => terrainData.SeedFilter(cell, region.LandCells, region.WaterCells)
                    ).Except(seeds);

                    if(seedCandidates.Count() == 0) {
                        Debug.LogWarning("Failed to assign expected number of seeds for terrain type " + terrain.ToString());
                        break;
                    }else {
                        var seed = WeightedRandomSampler<IHexCell>.SampleElementsFromSet(
                            seedCandidates, 1, terrainData.SeedWeightFunction
                        ).FirstOrDefault();

                        if(seed == null) {
                            Debug.LogWarning("Failed to select a seed from a nonzero number of candidates");
                            break;
                        }

                        seeds.Add(seed);

                        var crawler = GridTraversalLogic.GetCrawlingEnumerator(
                            seed, unassignedLandCells, acceptedCells, terrainData.CrawlingWeightFunction
                        );

                        crawlers.Add(crawler);
                    }
                }

                terrainCounts  [terrain] = terrainCount;
                terrainCrawlers[terrain] = crawlers;
                cellsOfTerrain [terrain] = acceptedCells;
            }

            int iterations = unassignedLandCells.Count * 10;
            while(unassignedLandCells.Count > 0 && iterations-- > 0) {
                var currentTerrain = landTerrains.Random();

                if(cellsOfTerrain[currentTerrain].Count < terrainCounts[currentTerrain]) {
                    var crawlers = terrainCrawlers[currentTerrain];

                    while(crawlers.Count > 0) {
                        var currentCrawler = crawlers.Random();
                        if(currentCrawler.MoveNext()) {
                            var newCell = currentCrawler.Current;

                            cellsOfTerrain[currentTerrain].Add(newCell);
                            unassignedLandCells.Remove(newCell);

                            break;

                        }else {
                            crawlers.Remove(currentCrawler);
                        }
                    }
                }
            }

            foreach(var terrain in landTerrains) {
                foreach(var cell in cellsOfTerrain[terrain]) {
                    ModLogic.ChangeTerrainOfCell(cell, terrain);
                }
            }

            foreach(var cell in region.WaterCells) {
                ModLogic.ChangeTerrainOfCell(cell, CellTerrain.ShallowWater);
            }
        }

        private void AssignFloodPlains(IEnumerable<IHexCell> landCells) {
            foreach(var desertCell in landCells.Where(cell => cell.Terrain == CellTerrain.Desert)) {
                if(ModLogic.CanChangeTerrainOfCell(desertCell, CellTerrain.FloodPlains)) {
                    ModLogic.ChangeTerrainOfCell(desertCell, CellTerrain.FloodPlains);
                }
            }
        }

        private void PaintVegetation(
            IEnumerable<IHexCell> landCells, IRegionTemplate template
        ) {
            var treeType = template.AreTreesJungle ? CellVegetation.Jungle : CellVegetation.Forest;

            var openCells = new List<IHexCell>();

            foreach(var cell in landCells) {
                if(ShouldBeMarsh(cell, template)) {
                    ModLogic.ChangeVegetationOfCell(cell, CellVegetation.Marsh);

                }else if(ModLogic.CanChangeVegetationOfCell(cell, treeType)) {
                   openCells.Add(cell);
                }
            }

            int treeCount = Mathf.RoundToInt(template.TreePercentage * openCells.Count * 0.01f);

            var treeSeeds = new List<IHexCell>();

            for(int i = 0; i < Random.Range(template.MinTreeClumps, template.MaxTreeClumps); i++) {
                treeSeeds.Add(openCells.Random());
            }

            var treeCells = new List<IHexCell>();

            var treeCrawlers = treeSeeds.Select(
                seed => GridTraversalLogic.GetCrawlingEnumerator(
                    seed, openCells, treeCells, GetTreeWeightFunction(treeType)
                )
            ).ToList();

            for(int i = 0; i < treeCount; i++) {
                if(treeCrawlers.Count == 0) {
                    Debug.LogWarning("Failed to paint correct number of trees into starting area");
                    break;
                }

                var crawler = treeCrawlers[Random.Range(0, treeCrawlers.Count)];

                if(crawler.MoveNext()) {
                    treeCells.Add(crawler.Current);
                    openCells.Remove(crawler.Current);
                }else {
                    treeCrawlers.Remove(crawler);
                    i--;
                }
            }

            foreach(var treeCell in treeCells) {
                ModLogic.ChangeVegetationOfCell(treeCell, treeType);
            }
        }

        private CrawlingWeightFunction GetTreeWeightFunction(CellVegetation treeType) {
            return delegate(IHexCell cell, IHexCell seed, IEnumerable<IHexCell> acceptedCells) {
                if( cell.Vegetation == CellVegetation.Marsh ||                    
                    !ModLogic.CanChangeVegetationOfCell(cell, treeType)
                ) {
                    return -1;

                }else{
                    return Grid.GetDistance(seed, cell);
                }
            };
        }

        private bool ShouldBeMarsh(IHexCell cell, IRegionTemplate template) {
            if(cell.Terrain != CellTerrain.Grassland || cell.Shape != CellShape.Flatlands) {
                return false;
            }

            int adjacentWater = Grid.GetNeighbors(cell).Where(
                neighbor => neighbor.Terrain.IsWater()
            ).Count();

            int adjacentRivers = EnumUtil.GetValues<HexDirection>().Where(
                direction => RiverCanon.HasRiverAlongEdge(cell, direction)
            ).Count();

            float chanceOfMarsh = template.MarshChanceBase
                                + adjacentWater  * template.MarshChancePerAdjacentWater
                                + adjacentRivers * template.MarshChancePerAdjacentRiver;

            return Random.value < chanceOfMarsh;
        }

        private int HillsStartingWeightFunction(IHexCell cell) {
            int weight = Random.Range(2, 3);

            if(Grid.GetNeighbors(cell).Exists(neighbor => neighbor.Terrain.IsWater())) {
                weight -= 1;
            }

            return weight;
        }

        private int HillsDynamicWeightFunction(IHexCell cell, List<IHexCell> elevatedCells) {
            int adjacentHillCount = Grid.GetNeighbors(cell).Where(adjacent => elevatedCells.Contains(adjacent)).Count();

            return 2 + adjacentHillCount < 2 ? 10 * adjacentHillCount : 5 - adjacentHillCount;
        }

        private int MountainWeightFunction(IHexCell cell) {
            return Grid.GetNeighbors(cell).Where(neighbor => neighbor.Shape != CellShape.Flatlands).Count();
        }

        #endregion
        
    }

}
