using System;
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
        private IRiverGenerator        RiverGenerator;
        private IResourceDistributor   ResourceDistributor;
        private IRegionBalancer        RegionBalancer;
        private IMapGenerationConfig   Config;
        private IVegetationPainter     VegetationPainter;

        #endregion

        #region constructors

        [Inject]
        public RegionGenerator(
            ICellModificationLogic modLogic, IHexGrid grid,
            IGridTraversalLogic gridTraversalLogic,
            IRiverGenerator riverGenerator, IResourceDistributor resourceDistributor,
            IRegionBalancer regionBalancer, IMapGenerationConfig config,
            IVegetationPainter vegetationPainter
        ) {
            ModLogic            = modLogic;
            Grid                = grid;
            GridTraversalLogic  = gridTraversalLogic;
            RiverGenerator      = riverGenerator;
            ResourceDistributor = resourceDistributor;
            RegionBalancer      = regionBalancer;
            Config              = config;
            VegetationPainter   = vegetationPainter;
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

            VegetationPainter.PaintVegetation(region, template);
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

        private void PaintTerrain(MapRegion region, IRegionTemplate template) {
            var unassignedLandCells = new HashSet<IHexCell>(region.LandCells);

            PaintArcticTerrain   (region, template, unassignedLandCells);
            PaintNonArcticTerrain(region, template, unassignedLandCells);
            AssignLandOrphans    (region, template, unassignedLandCells);

            foreach(var cell in region.WaterCells) {
                ModLogic.ChangeTerrainOfCell(cell, CellTerrain.ShallowWater);
            }
        }

        private void PaintNonArcticTerrain(
            MapRegion region, IRegionTemplate template, HashSet<IHexCell> unassignedLandCells
        ) {
            var nonArcticLandTerrains = EnumUtil.GetValues<CellTerrain>().Where(
                terrain => !terrain.IsWater() && !terrain.IsArctic()
            );

            var dataOfTerrains  = new Dictionary<CellTerrain, TerrainData>();
            var terrainCounts   = new Dictionary<CellTerrain, int>();
            var terrainCrawlers = new Dictionary<CellTerrain, List<IEnumerator<IHexCell>>>();
            var cellsOfTerrain  = new Dictionary<CellTerrain, List<IHexCell>>();
            
            var seeds = new List<IHexCell>();

            foreach(var terrain in nonArcticLandTerrains) {
                var terrainData = template.GetNonArcticTerrainData(terrain);
                dataOfTerrains[terrain] = terrainData;

                var terrainCount = Mathf.RoundToInt(region.LandCells.Count * terrainData.Percentage * 0.01f);

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
                var currentTerrain = nonArcticLandTerrains.Random();

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

            foreach(var terrain in nonArcticLandTerrains) {
                foreach(var cell in cellsOfTerrain[terrain]) {
                    ModLogic.ChangeTerrainOfCell(cell, terrain);
                }
            }
        }

        private void PaintArcticTerrain(
            MapRegion region, IRegionTemplate template, HashSet<IHexCell> unassignedLandCells
        ) {
            var unassignedByPolarDistance = new List<IHexCell>(unassignedLandCells);

            unassignedByPolarDistance.Sort(PolarDistanceComparer);

            int snowCellCount   = Mathf.RoundToInt(template.SnowPercentage   * region.LandCells.Count * 0.01f);
            int tundraCellCount = Mathf.RoundToInt(template.TundraPercentage * region.LandCells.Count * 0.01f);

            for(int i = 0; i < snowCellCount; i++) {
                if(unassignedByPolarDistance.Any()) {
                    var candidate = unassignedByPolarDistance.Last();

                    if(ModLogic.CanChangeTerrainOfCell(candidate, CellTerrain.Snow)) {
                        ModLogic.ChangeTerrainOfCell(candidate, CellTerrain.Snow);
                        unassignedLandCells.Remove(candidate);
                    }

                    unassignedLandCells.Remove(candidate);
                    unassignedByPolarDistance.RemoveAt(unassignedByPolarDistance.Count - 1);
                }else {
                    break;
                }
            }

            for(int i = 0; i < tundraCellCount; i++) {
                if(unassignedByPolarDistance.Any()) {
                    var candidate = unassignedByPolarDistance.Last();

                    if(ModLogic.CanChangeTerrainOfCell(candidate, CellTerrain.Tundra)) {
                        ModLogic.ChangeTerrainOfCell(candidate, CellTerrain.Tundra);
                        unassignedLandCells.Remove(candidate);
                    }
                    
                    unassignedByPolarDistance.RemoveAt(unassignedByPolarDistance.Count - 1);
                }else {
                    break;
                }
            }
        }

        private void AssignLandOrphans(
            MapRegion region, IRegionTemplate template, HashSet<IHexCell> unassignedLandCells
        ) {
            foreach(var orphan in unassignedLandCells.ToArray()) {
                var adjacentLandTerrains = Grid.GetCellsInRadius(orphan, 3)
                                               .Except(unassignedLandCells)
                                               .Intersect(region.LandCells)
                                               .Select(neighbor => neighbor.Terrain)
                                               .Where(terrain => !terrain.IsWater() && ModLogic.CanChangeTerrainOfCell(orphan, terrain));

                if(adjacentLandTerrains.Any()) {
                    var newTerrain = adjacentLandTerrains.Random();

                    ModLogic.ChangeTerrainOfCell(orphan, newTerrain);

                    unassignedLandCells.Remove(orphan);
                }else {
                    Debug.LogWarning("Could not find a valid terrain for an orphaned cell");
                }
            }
        }

        private void AssignFloodPlains(IEnumerable<IHexCell> landCells) {
            foreach(var desertCell in landCells.Where(cell => cell.Terrain == CellTerrain.Desert)) {
                if(ModLogic.CanChangeTerrainOfCell(desertCell, CellTerrain.FloodPlains)) {
                    ModLogic.ChangeTerrainOfCell(desertCell, CellTerrain.FloodPlains);
                }
            }
        }

        private int HillsStartingWeightFunction(IHexCell cell) {
            int weight = UnityEngine.Random.Range(2, 3);

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

        private int PolarDistanceComparer(IHexCell cellA, IHexCell cellB) {
            int polarDistanceA, polarDistanceB;

            int zOffsetA = HexCoordinates.ToOffsetCoordinateZ(cellA.Coordinates);
            int zOffsetB = HexCoordinates.ToOffsetCoordinateZ(cellB.Coordinates);

            if(Config.Hemispheres == HemisphereMode.Both) {
                polarDistanceA = Math.Min(zOffsetA, Grid.CellCountZ - zOffsetA);
                polarDistanceB = Math.Min(zOffsetB, Grid.CellCountZ - zOffsetB);

            }else if(Config.Hemispheres == HemisphereMode.North) {
                polarDistanceA = Grid.CellCountZ - zOffsetA;
                polarDistanceB = Grid.CellCountZ - zOffsetB;

            }else if(Config.Hemispheres == HemisphereMode.South) {
                polarDistanceA = zOffsetA;
                polarDistanceB = zOffsetB;

            }else {
                throw new NotImplementedException("No behavior defined for HemisphereMode " + Config.Hemispheres);
            }

            return polarDistanceB.CompareTo(polarDistanceA);
        }

        #endregion
        
    }

}
