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
        private IResourceNodeFactory   NodeFactory;

        #endregion

        #region constructors

        [Inject]
        public RegionGenerator(
            ICellModificationLogic modLogic, IHexGrid grid, ICellTemperatureLogic temperatureLogic,
            IGridTraversalLogic gridTraversalLogic, IRiverCanon riverCanon,
            IRiverGenerator riverGenerator, IResourceNodeFactory nodeFactory
        ) {
            ModLogic           = modLogic;
            Grid               = grid;
            TemperatureLogic   = temperatureLogic;
            GridTraversalLogic = gridTraversalLogic;
            RiverCanon         = riverCanon;
            RiverGenerator     = riverGenerator;
            NodeFactory        = nodeFactory;
        }

        #endregion

        #region instance methods

        #region from IStartingLocationGenerator

        public void GenerateRegion(
            MapRegion region, IRegionGenerationTemplate template,
            IEnumerable<IHexCell> oceanCells,
            List<IResourceDefinition> availableLuxuries
        ) {
            GenerateTopology(region, template);
            PaintTerrain(region, template, oceanCells);

            RiverGenerator.CreateRiversForRegion(region, template, oceanCells);

            PaintVegetation(region, template);
        }

        #endregion

        private void GenerateTopology(MapRegion region, IRegionGenerationTemplate template) {
            int desiredMountainCount = Mathf.RoundToInt(template.MountainsPercentage * region.Cells.Count * 0.01f);
            int desiredHillsCount    = Mathf.RoundToInt(template.HillsPercentage     * region.Cells.Count * 0.01f);

            var elevatedCells = WeightedRandomSampler<IHexCell>.SampleElementsFromSet(
                region.Cells, desiredHillsCount + desiredMountainCount,
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
            MapRegion region, IRegionGenerationTemplate template,
            IEnumerable<IHexCell> oceanCells
        ) {
            var terrains = EnumUtil.GetValues<CellTerrain>();

            var dataOfTerrains  = new Dictionary<CellTerrain, TerrainData>();
            var terrainCounts   = new Dictionary<CellTerrain, int>();
            var terrainCrawlers = new Dictionary<CellTerrain, List<IEnumerator<IHexCell>>>();
            var cellsOfTerrain  = new Dictionary<CellTerrain, List<IHexCell>>();

            var unassignedCells = new HashSet<IHexCell>(region.Cells);
            var seeds = new List<IHexCell>();

            foreach(var terrain in terrains) {
                var terrainData = template.GetTerrainData(terrain);
                dataOfTerrains[terrain] = terrainData;

                var terrainCount = Mathf.RoundToInt(region.Cells.Count * terrainData.Percentage * 0.01f);

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

            foreach(var cell in region.Cells) {
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

        private void PaintVegetation(MapRegion region, IRegionGenerationTemplate template) {
            var openCells = new List<IHexCell>();

            foreach(var cell in region.Cells) {
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
