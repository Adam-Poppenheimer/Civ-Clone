using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.MapResources;
using Assets.Simulation.HexMap;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapGeneration {

    public class RegionGenerator : IRegionGenerator {

        #region instance fields and properties

        private ICellModificationLogic ModLogic;
        private IHexGrid               Grid;
        private ICellTemperatureLogic  TemperatureLogic;
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
            ICellModificationLogic modLogic, IHexGrid grid, ICellTemperatureLogic temperatureLogic,
            IGridTraversalLogic gridTraversalLogic, IRiverCanon riverCanon,
            IRiverGenerator riverGenerator, IResourceDistributor resourceDistributor,
            IRegionBalancer regionBalancer, IMapGenerationConfig config
        ) {
            ModLogic            = modLogic;
            Grid                = grid;
            TemperatureLogic    = temperatureLogic;
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

        public void GenerateRegion(
            MapRegion region, IRegionGenerationTemplate template,
            IEnumerable<IHexCell> oceanCells
        ) {
            var waterCells = new List<IHexCell>(CarveOutCoasts(region, oceanCells));

            foreach(var cell in waterCells) {
                ModLogic.ChangeTerrainOfCell(cell, CellTerrain.DeepWater);
            }

            var undecidedCells = region.Cells.Except(waterCells);

            GenerateTopology(undecidedCells, template);
            PaintTerrain(undecidedCells, template, oceanCells);

            var newWaterCells = undecidedCells.Where(cell => cell.Terrain.IsWater());

            waterCells.AddRange(newWaterCells);

            var landCells = undecidedCells.Except(newWaterCells);

            RationalizeWater(waterCells, oceanCells);

            RiverGenerator.CreateRiversForRegion(landCells, template, oceanCells);

            PaintVegetation(landCells, template);

            ResourceDistributor.DistributeLuxuryResourcesAcrossRegion(region, template, oceanCells);
            RegionBalancer.BalanceRegionYields(region, template, oceanCells);
        }

        #endregion

        IEnumerable<IHexCell> CarveOutCoasts(MapRegion region, IEnumerable<IHexCell> oceanCells) {
            return region.Cells.Where(
                cell => Grid.GetCellsInRadius(cell, Config.ContinentalShelfDistance).Any(
                    nearby => oceanCells.Contains(nearby)
                )
            ).ToList();
        }

        private void GenerateTopology(IEnumerable<IHexCell> undecidedCells, IRegionGenerationTemplate template) {
            int desiredMountainCount = Mathf.RoundToInt(template.MountainsPercentage * undecidedCells.Count() * 0.01f);
            int desiredHillsCount    = Mathf.RoundToInt(template.HillsPercentage     * undecidedCells.Count() * 0.01f);

            var elevatedCells = WeightedRandomSampler<IHexCell>.SampleElementsFromSet(
                undecidedCells, desiredHillsCount + desiredMountainCount,
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
            IEnumerable<IHexCell> undecidedCells, IRegionGenerationTemplate template,
            IEnumerable<IHexCell> oceanCells
        ) {
            var terrains = EnumUtil.GetValues<CellTerrain>();

            var dataOfTerrains  = new Dictionary<CellTerrain, TerrainData>();
            var terrainCounts   = new Dictionary<CellTerrain, int>();
            var terrainCrawlers = new Dictionary<CellTerrain, List<IEnumerator<IHexCell>>>();
            var cellsOfTerrain  = new Dictionary<CellTerrain, List<IHexCell>>();

            var unassignedCells = new HashSet<IHexCell>(undecidedCells);
            var seeds = new List<IHexCell>();

            foreach(var terrain in terrains) {
                var terrainData = template.GetTerrainData(terrain);
                dataOfTerrains[terrain] = terrainData;

                var terrainCount = Mathf.RoundToInt(undecidedCells.Count() * terrainData.Percentage * 0.01f);

                var acceptedCells = new List<IHexCell>();

                var crawlers = new List<IEnumerator<IHexCell>>();

                for(int i = 0; i < terrainData.SeedCount; i++) {
                    var seedCandidates = unassignedCells.Where(cell => terrainData.SeedFilter(cell, oceanCells)).Except(seeds);

                    if(seedCandidates.Count() == 0) {
                        Debug.LogWarning("Failed to assign expected number of seeds for terrain type " + terrain.ToString());
                        break;
                    }else {
                        var seed = seedCandidates.Random();
                        seeds.Add(seed);

                        var crawler = GridTraversalLogic.GetCrawlingEnumerator(
                            seed, unassignedCells, acceptedCells, terrainData.WeightFunction
                        );

                        crawlers.Add(crawler);
                    }
                }

                terrainCounts  [terrain] = terrainCount;
                terrainCrawlers[terrain] = crawlers;
                cellsOfTerrain [terrain] = acceptedCells;
            }

            foreach(var cell in undecidedCells) {
                foreach(var terrain in terrains) {
                    if(dataOfTerrains[terrain].ForceAdaptFilter(cell)) {

                        cellsOfTerrain[terrain].Add(cell);
                        unassignedCells.Remove(cell);

                        break;
                    }
                }
            }

            int iterations = unassignedCells.Count * 10;
            while(unassignedCells.Count > 0 && iterations-- > 0) {
                var currentTerrain = terrains.Random();

                if(cellsOfTerrain[currentTerrain].Count < terrainCounts[currentTerrain]) {
                    var crawlers = terrainCrawlers[currentTerrain];

                    while(crawlers.Count > 0) {
                        var currentCrawler = crawlers.Random();
                        if(currentCrawler.MoveNext()) {
                            var newCell = currentCrawler.Current;

                            cellsOfTerrain[currentTerrain].Add(newCell);
                            unassignedCells.Remove(newCell);

                            break;

                        }else {
                            crawlers.Remove(currentCrawler);
                        }
                    }
                }
            }

            foreach(var terrain in terrains) {
                foreach(var cell in cellsOfTerrain[terrain]) {
                    ModLogic.ChangeTerrainOfCell(cell, terrain);
                }
            }
        }

        private void RationalizeWater(
            IEnumerable<IHexCell> waterCells, IEnumerable<IHexCell> oceanCells
        ) {
            foreach(var waterCell in waterCells) {
                var landCandidates = Grid.GetCellsInRadius(waterCell, Config.ContinentalShelfDistance);

                if(landCandidates.Any(cell => !cell.Terrain.IsWater() && !oceanCells.Contains(cell))) {
                    ModLogic.ChangeTerrainOfCell(waterCell, CellTerrain.ShallowWater);
                }else {
                    ModLogic.ChangeTerrainOfCell(waterCell, CellTerrain.DeepWater);
                }
            }
        }

        private void PaintVegetation(
            IEnumerable<IHexCell> landCells, IRegionGenerationTemplate template
        ) {
            var openCells = new List<IHexCell>();

            foreach(var cell in landCells) {
                if(ShouldBeMarsh(cell, template)) {
                    ModLogic.ChangeVegetationOfCell(cell, CellVegetation.Marsh);

                }else if(cell.Terrain.IsWater() || cell.Terrain == CellTerrain.Snow) {
                    continue;

                }else {
                    openCells.Add(cell);
                }
            }

            int treeCount = Mathf.RoundToInt(template.TreePercentage * openCells.Count * 0.01f);

            var treeSeeds = new List<IHexCell>();

            for(int i = 0; i < Random.Range(template.MinTreeClumps, template.MaxTreeClumps); i++) {
                treeSeeds.Add(openCells[Random.Range(0, openCells.Count)]);
            }

            var treeCells = new List<IHexCell>();

            var treeCrawlers = treeSeeds.Select(
                seed => GridTraversalLogic.GetCrawlingEnumerator(
                    seed, openCells, treeCells, TreeWeightFunction
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

            int jitterChannel = Random.Range(0, 4);

            foreach(var treeCell in treeCells) {
                float temperature = TemperatureLogic.GetTemperatureOfCell(treeCell, jitterChannel);

                if( temperature >= template.JungleThreshold &&
                    ModLogic.CanChangeVegetationOfCell(treeCell, CellVegetation.Jungle)
                ) {
                    ModLogic.ChangeVegetationOfCell(treeCell, CellVegetation.Jungle);
                }else if(ModLogic.CanChangeVegetationOfCell(treeCell, CellVegetation.Forest)) {
                    ModLogic.ChangeVegetationOfCell(treeCell, CellVegetation.Forest);
                }
            }
        }

        private int TreeWeightFunction(IHexCell cell, IHexCell seed, IEnumerable<IHexCell> acceptedCells) {
            if( cell.Vegetation == CellVegetation.Marsh ||                    
                !ModLogic.CanChangeVegetationOfCell(cell, CellVegetation.Forest)
            ) {
                return -1;

            }else{
                return Grid.GetDistance(seed, cell);
            }
        }

        private bool ShouldBeMarsh(IHexCell cell, IRegionGenerationTemplate template) {
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
