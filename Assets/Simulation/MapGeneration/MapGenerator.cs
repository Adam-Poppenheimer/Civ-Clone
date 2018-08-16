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

    public class MapGenerator : IMapGenerator {

        #region instance fields and properties

        private IMapGenerationConfig        Config;
        private ICivilizationFactory        CivFactory;
        private IHexGrid                    Grid;
        private IRegionGenerator            RegionGenerator;
        private IOceanGenerator             OceanGenerator;
        private IGridTraversalLogic         GridTraversalLogic;
        private IResourceSampler            ResourceSampler;
        private ICellModificationLogic      ModLogic;
        private IStartingUnitPlacementLogic StartingUnitPlacementLogic;

        #endregion

        #region constructors

        [Inject]
        public MapGenerator(
            IMapGenerationConfig config, ICivilizationFactory civFactory,
            IHexGrid grid, IRegionGenerator regionGenerator,
            IOceanGenerator oceanGenerator, IGridTraversalLogic gridTraversalLogic,
            IResourceSampler resourceSampler, ICellModificationLogic modLogic,
            IStartingUnitPlacementLogic startingUnitPlacementLogic
        ) {
            Config                     = config;
            CivFactory                 = civFactory;
            Grid                       = grid;
            RegionGenerator            = regionGenerator;
            OceanGenerator             = oceanGenerator;
            GridTraversalLogic         = gridTraversalLogic;
            ResourceSampler            = resourceSampler;
            ModLogic                   = modLogic;
            StartingUnitPlacementLogic = startingUnitPlacementLogic;
        }

        #endregion

        #region instance methods

        #region from IHexMapGenerator

        public void GenerateMap(int chunkCountX, int chunkCountZ, IMapGenerationTemplate template) {
            ResourceSampler.Reset();

            var oldRandomState = SetRandomState();

            Grid.Build(chunkCountX, chunkCountZ);

            GenerateCivs(template.CivCount);

            List<MapRegion> landRegions;
            List<MapSection> oceanSections;

            GenerateOceansAndContinents(template, out landRegions, out oceanSections);

            PaintMap(landRegions, oceanSections, template);

            for(int i = 0; i < CivFactory.AllCivilizations.Count; i++) {
                StartingUnitPlacementLogic.PlaceStartingUnitsInRegion(
                    landRegions[i], CivFactory.AllCivilizations[i], template
                );

                float mapData = (float)i / (CivFactory.AllCivilizations.Count - 1);
                foreach(var cell in landRegions[i].Cells) {
                    cell.SetMapData(mapData);
                }
            }

            UnityEngine.Random.state = oldRandomState;
        }

        #endregion

        private UnityEngine.Random.State SetRandomState() {
            var originalState = UnityEngine.Random.state;

            if(!Config.UseFixedSeed) {
                int seed = UnityEngine.Random.Range(0, int.MaxValue);
                seed ^= (int)System.DateTime.Now.Ticks;
                seed ^= (int)Time.unscaledTime;
                seed &= int.MaxValue;
                UnityEngine.Random.InitState(seed);

                Debug.Log("Using seed " + seed);
            }else {
                UnityEngine.Random.InitState(Config.FixedSeed);
            } 

            return originalState;
        }

        private void GenerateCivs(int civCount) {
            CivFactory.Clear();

            for(int i = 0; i < civCount; i++) {
                CivFactory.Create(string.Format("Civ {0}", i + 1), UnityEngine.Random.ColorHSV());
            }
        }

        private void GenerateOceansAndContinents(
            IMapGenerationTemplate template, out List<MapRegion> landRegions,
            out List<MapSection> oceanSections
        ) {
            var partition = GetVoronoiPartitionOfCells(Grid.AllCells, template);     
            
            int totalLandCells = Mathf.RoundToInt(Grid.AllCells.Count * template.ContinentalLandPercentage * 0.01f);
            int landCellsPerCiv = Mathf.RoundToInt((float)totalLandCells / CivFactory.AllCivilizations.Count);       

            HashSet<MapSection> unassignedSections = new HashSet<MapSection>(partition.Sections);

            var continentOfCiv = new Dictionary<ICivilization, Continent>();
            var startingSections = new List<MapSection>();

            var unfinishedCivs = new List<ICivilization>(CivFactory.AllCivilizations);
            foreach(var civ in unfinishedCivs) {
                var startingSection = GetStartingSection(unassignedSections, startingSections, template);

                continentOfCiv[civ] = new Continent(startingSection);

                startingSections.Add(startingSection);

                unassignedSections.Remove(startingSection);
            }

            while(unfinishedCivs.Any() && unassignedSections.Any()) {
                var civ = unfinishedCivs.Random();

                var continent = continentOfCiv[civ];

                if(TryExpandContinent(continent, unassignedSections, partition, template)) {
                    int cellsBelongingToCiv = continent.Sections.Sum(section => section.Cells.Count);

                    if(cellsBelongingToCiv >= landCellsPerCiv) {
                        unfinishedCivs.Remove(civ);
                    }
                }else {
                    Debug.LogWarning("Failed to assign new section to civ " + civ.Name);
                    unfinishedCivs.Remove(civ);
                }
            }

            landRegions = new List<MapRegion>();

            foreach(var continent in continentOfCiv.Values) {
                var land = new MapSection(Grid);

                foreach(var cell in continent.Sections.SelectMany(section => section.Cells)) {
                    land.AddCell(cell);
                }

                var water = GetCoastForLandSection(land, unassignedSections, partition);

                var region = new MapRegion(land, water);

                landRegions.Add(region);
            }

            var ocean = new MapSection(Grid);

            foreach(var unchosenCell in unassignedSections.SelectMany(section => section.Cells)) {
                if(unchosenCell != null) {
                    ocean.AddCell(unchosenCell);
                }
            }

            oceanSections = new List<MapSection>() { ocean };
        }

        private void PaintMap(
            List<MapRegion> landRegions, List<MapSection> oceanSections,
            IMapGenerationTemplate mapTemplate
        ) {
            var templatesOfRegion = new Dictionary<MapRegion, IRegionGenerationTemplate>();

            foreach(var landRegion in landRegions) {
                var regionTemplate = GetAppropriateLandTemplate(mapTemplate, landRegion);

                templatesOfRegion[landRegion] = regionTemplate;

                RegionGenerator.GenerateTopologyAndEcology(landRegion, regionTemplate);
            }

            foreach(var oceanSection in oceanSections) {
                var oceanTemplate = GetAppropriateOceanTemplate(mapTemplate, oceanSection);

                OceanGenerator.GenerateOcean(oceanSection, oceanTemplate);
            }

            RationalizeWater();

            foreach(var landRegion in landRegions) {
                var regionTemplate = templatesOfRegion[landRegion];

                RegionGenerator.DistributeYieldAndResources(landRegion, regionTemplate);
            }
        }

        private MapSection GetCoastForLandSection(
            MapSection land, HashSet<MapSection> unassignedSections, VoronoiPartition partition
        ) {
            var cellsAdjacentToLand = land.Cells.SelectMany(cell => Grid.GetNeighbors(cell))
                                                  .Distinct()
                                                  .Except(land.Cells);

            var sectionsAdjacentToland = cellsAdjacentToLand.Select(cell => partition.GetSectionOfCell(cell))
                                                            .Intersect(unassignedSections)
                                                            .Distinct()
                                                            .ToList();

            foreach(var adjacentSection in sectionsAdjacentToland) {
                unassignedSections.Remove(adjacentSection);
            }

            var coastalCells = sectionsAdjacentToland.SelectMany(section => section.Cells).ToList();

            var coastRegion = new MapSection(Grid);

            foreach(var coastalCell in coastalCells) {
                coastRegion.AddCell(coastalCell);
            }

            return coastRegion;
        }

        private VoronoiPartition GetVoronoiPartitionOfCells(
            IEnumerable<IHexCell> allCells, IMapGenerationTemplate template
        ) {
            float xMin = 0f, zMin = 0f;
            float xMax = (Grid.CellCountX + Grid.CellCountZ * 0.5f - Grid.CellCountZ / 2) * HexMetrics.InnerRadius * 2f;
            float zMax = Grid.CellCountZ * HexMetrics.OuterRadius * 1.5f;

            var randomPoints = new List<Vector3>();

            var regionOfPoint = new Dictionary<Vector3, MapSection>();

            for(int i = 0; i < template.VoronoiPointCount; i++) {
                var randomPoint = new Vector3(
                    UnityEngine.Random.Range(xMin, xMax),
                    0f,
                    UnityEngine.Random.Range(zMin, zMax)
                );

                regionOfPoint[randomPoint] = new MapSection(Grid);

                randomPoints.Add(randomPoint);
            }

            int iterationsLeft = template.VoronoiPartitionIterations;
            while(iterationsLeft > 0) {
                foreach(var cell in allCells) {
                    Vector3 nearestPoint = Vector3.zero;
                    float shorestDistance = float.MaxValue;

                    foreach(var voronoiPoint in regionOfPoint.Keys) {
                        float distanceTo = Vector3.Distance(cell.LocalPosition, voronoiPoint);

                        if(distanceTo < shorestDistance) {
                            nearestPoint = voronoiPoint;
                            shorestDistance = distanceTo;
                        }
                    }

                    if(regionOfPoint.ContainsKey(nearestPoint)) {
                        regionOfPoint[nearestPoint].AddCell(cell);
                    }                
                }

                if(--iterationsLeft > 0) {
                    randomPoints.Clear();

                    var newRegionOfPoints = new Dictionary<Vector3, MapSection>();
                    foreach(var region in regionOfPoint.Values) {
                        if(region.Cells.Count > 0) {
                            randomPoints.Add(region.Centroid);

                            newRegionOfPoints[region.Centroid] = new MapSection(Grid);
                        }
                    }

                    regionOfPoint = newRegionOfPoints;
                }
            }

            return new VoronoiPartition(regionOfPoint.Values, Grid);
        }

        private MapSection GetStartingSection(
            HashSet<MapSection> unassignedSections, IEnumerable<MapSection> startingSections,
            IMapGenerationTemplate template
        ) {
            var candidates = new List<MapSection>();

            foreach(var unassignedSection in unassignedSections) {
                if(unassignedSection.Cells.Count == 0) {
                    continue;
                }

                bool unassignedIsValid = true;
                foreach(var startingSection in startingSections) {
                    if( Grid.GetDistance(unassignedSection.CentroidCell, startingSection.CentroidCell) < template.MinStartingLocationDistance ||
                        IsWithinSoftBorder(unassignedSection.CentroidCell)
                    ) {
                        unassignedIsValid = false;
                        break;
                    }
                }

                if(unassignedIsValid) {
                    candidates.Add(unassignedSection);
                }
            }

            if(candidates.Count == 0) {
                throw new InvalidOperationException("Failed to acquire a valid starting location");
            }else {
                return candidates.Random();
            }
        }

        private bool TryExpandContinent(
            Continent continent, HashSet<MapSection> unassignedSections,
            VoronoiPartition partition, IMapGenerationTemplate template
        ) {
            var expansionCandiates = new HashSet<MapSection>();

            foreach(var section in continent.Sections) {
                foreach(var neighbor in partition.GetNeighbors(section)) {
                    if(unassignedSections.Contains(neighbor)) {
                        expansionCandiates.Add(neighbor);
                    }
                }
            }

            if(expansionCandiates.Any()) {
                var newSection = WeightedRandomSampler<MapSection>.SampleElementsFromSet(
                    expansionCandiates, 1, GetExpansionWeightFunction(
                        continent, partition, template
                    )
                ).FirstOrDefault();

                if(newSection != null) {
                    unassignedSections.Remove(newSection);
                    continent.AddSection(newSection);

                    return true;
                }             
            }

            return false;
        }

        private Func<MapSection, int> GetExpansionWeightFunction(
            Continent continent, VoronoiPartition partition,
            IMapGenerationTemplate template
        ) {
            return delegate(MapSection region) {
                if(IsWithinHardBorder(region.CentroidCell)) {
                    return 0;
                }

                int borderAvoidanceWeight = GetBorderAvoidanceWeight(region.CentroidCell);

                int neighborsInContinent = partition.GetNeighbors(region).Intersect(continent.Sections).Count();
                int neighborsInContinentWeight = neighborsInContinent * template.NeighborsInContinentWeight;

                int distanceFromSeedCentroid = Grid.GetDistance(continent.Seed.CentroidCell, region.CentroidCell);
                int distanceFromSeedCentroidWeight = distanceFromSeedCentroid * template.DistanceFromSeedCentroidWeight;

                int weight = 1 + Math.Max(0, neighborsInContinentWeight + distanceFromSeedCentroidWeight + borderAvoidanceWeight);

                return weight;
            };
        }

        private bool IsWithinHardBorder(IHexCell cell) {
            var xOffset = HexCoordinates.ToOffsetCoordinateX(cell.Coordinates);
            var zOffset = HexCoordinates.ToOffsetCoordinateZ(cell.Coordinates);

            return xOffset <= Config.HardMapBorderX || Grid.CellCountX - xOffset <= Config.HardMapBorderX
                || zOffset <= Config.HardMapBorderZ || Grid.CellCountZ - zOffset <= Config.HardMapBorderZ;
        }

        private bool IsWithinSoftBorder(IHexCell cell) {
            var xOffset = HexCoordinates.ToOffsetCoordinateX(cell.Coordinates);
            var zOffset = HexCoordinates.ToOffsetCoordinateZ(cell.Coordinates);

            return xOffset <= Config.SoftMapBorderX || Grid.CellCountX - xOffset <= Config.SoftMapBorderX
                || zOffset <= Config.SoftMapBorderZ || Grid.CellCountZ - zOffset <= Config.SoftMapBorderZ;
        }

        private int GetBorderAvoidanceWeight(IHexCell cell) {
            int distanceIntoBorderX = 0, distanceIntoBorderZ = 0;

            var xOffset = HexCoordinates.ToOffsetCoordinateX(cell.Coordinates);
            var zOffset = HexCoordinates.ToOffsetCoordinateZ(cell.Coordinates);

            if(xOffset < Config.SoftMapBorderX) {
                distanceIntoBorderX = Config.SoftMapBorderX - xOffset;

            }else if(Grid.CellCountX - xOffset < Config.SoftMapBorderX) {
                distanceIntoBorderX = Config.SoftMapBorderX - (Grid.CellCountX - xOffset);
            }

            if(zOffset < Config.SoftMapBorderZ) {
                distanceIntoBorderZ = Config.SoftMapBorderZ - zOffset;

            }else if(Grid.CellCountZ - zOffset < Config.SoftMapBorderZ) {
                distanceIntoBorderZ = Config.SoftMapBorderZ - (Grid.CellCountZ - zOffset);
            }

            return -(distanceIntoBorderX + distanceIntoBorderZ) * Config.SoftBorderAvoidanceWeight;
        }

        private IRegionGenerationTemplate GetAppropriateLandTemplate(
            IMapGenerationTemplate mapTemplate, MapRegion landRegion
        ) {
            return mapTemplate.CivRegionTemplates.Random();
        }

        private IOceanGenerationTemplate GetAppropriateOceanTemplate(
            IMapGenerationTemplate mapTemplate, MapSection oceanSection
        ) {
            return mapTemplate.OceanTemplates.Random();
        }

        private void RationalizeWater() {
            var unrationalizedWater = Grid.AllCells.Where(cell => cell.Terrain.IsWater()).ToList();

            while(unrationalizedWater.Count > 0) {
                var waterBodySeed = unrationalizedWater[0];
                
                var cellsInWaterBody = new HashSet<IHexCell>();

                var waterBodyCrawler = GridTraversalLogic.GetCrawlingEnumerator(
                    waterBodySeed, unrationalizedWater, cellsInWaterBody,
                    WaterBodyWeightFunction
                );

                while(waterBodyCrawler.MoveNext()) {
                    cellsInWaterBody.Add(waterBodyCrawler.Current);
                    unrationalizedWater.Remove(waterBodyCrawler.Current);
                }

                if(cellsInWaterBody.Count <= Config.MaxLakeSize) {
                    foreach(var cell in cellsInWaterBody) {
                        ModLogic.ChangeTerrainOfCell(cell, CellTerrain.FreshWater);
                    }
                }
            }
        }

        private int WaterBodyWeightFunction(IHexCell cell, IHexCell seed, IEnumerable<IHexCell> acceptedCells) {
            return cell.Terrain.IsWater() && !acceptedCells.Contains(cell) ? 1 : -1;
        }

        #endregion
        
    }

}
