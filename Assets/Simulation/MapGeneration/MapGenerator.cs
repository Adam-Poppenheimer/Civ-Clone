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

        #region internal types

        private class OceanAndContinentData {

            public Dictionary<ICivilization, CivHomelandData> HomelandDataForCiv;
            public OceanData                                  OceanData;

        }

        #endregion

        #region instance fields and properties

        private IMapGenerationConfig        Config;
        private ICivilizationFactory        CivFactory;
        private IHexGrid                    Grid;
        private IOceanGenerator             OceanGenerator;
        private IResourceSampler            ResourceSampler;
        private IStartingUnitPlacementLogic StartingUnitPlacementLogic;
        private IGridPartitionLogic         GridPartitionLogic;
        private IWaterRationalizer          WaterRationalizer;
        private ICivHomelandGenerator       HomelandGenerator;
        private ITemplateSelectionLogic     TemplateSelectionLogic;

        #endregion

        #region constructors

        [Inject]
        public MapGenerator(
            IMapGenerationConfig config, ICivilizationFactory civFactory, IHexGrid grid,
            IOceanGenerator oceanGenerator, IGridTraversalLogic gridTraversalLogic,
            IResourceSampler resourceSampler, IStartingUnitPlacementLogic startingUnitPlacementLogic,
            IGridPartitionLogic gridPartitionLogic, IWaterRationalizer waterRationalizer,
            ICivHomelandGenerator homelandGenerator, ITemplateSelectionLogic templateSelectionLogic
        ) {
            Config                     = config;
            CivFactory                 = civFactory;
            Grid                       = grid;
            OceanGenerator             = oceanGenerator;
            ResourceSampler            = resourceSampler;
            StartingUnitPlacementLogic = startingUnitPlacementLogic;
            GridPartitionLogic         = gridPartitionLogic;
            WaterRationalizer          = waterRationalizer;
            HomelandGenerator          = homelandGenerator;
            TemplateSelectionLogic     = templateSelectionLogic;
        }

        #endregion

        #region instance methods

        #region from IHexMapGenerator

        public void GenerateMap(int chunkCountX, int chunkCountZ, IMapTemplate template) {
            ResourceSampler.Reset();

            var oldRandomState = SetRandomState();

            Grid.Build(chunkCountX, chunkCountZ);

            GenerateCivs(template.CivCount);

            var oceansAndContinents = GenerateOceansAndContinents(template);

            PaintMap(oceansAndContinents, template);

            foreach(var civ in oceansAndContinents.HomelandDataForCiv.Keys) {
                var civHomeland = oceansAndContinents.HomelandDataForCiv[civ];

                StartingUnitPlacementLogic.PlaceStartingUnitsInRegion(
                    civHomeland.StartingRegion, civ, template
                );
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

        private OceanAndContinentData GenerateOceansAndContinents(IMapTemplate mapTemplate) {
            var partition = GridPartitionLogic.GetPartitionOfGrid(Grid, mapTemplate);
            
            int totalLandCells = Mathf.RoundToInt(Grid.Cells.Count * mapTemplate.ContinentalLandPercentage * 0.01f);
            int landCellsPerCiv = Mathf.RoundToInt((float)totalLandCells / CivFactory.AllCivilizations.Count);       

            HashSet<MapSection> unassignedSections = new HashSet<MapSection>(partition.Sections);

            var landSectionsOfCivs = new Dictionary<ICivilization, List<MapSection>>();
            var startingSections = new List<MapSection>();

            var unfinishedCivs = new List<ICivilization>(CivFactory.AllCivilizations);
            foreach(var civ in unfinishedCivs) {
                var startingSection = GetStartingSection(unassignedSections, startingSections, mapTemplate);

                landSectionsOfCivs[civ] = new List<MapSection>() { startingSection };

                startingSections.Add(startingSection);

                unassignedSections.Remove(startingSection);
            }

            while(unfinishedCivs.Any() && unassignedSections.Any()) {
                var civ = unfinishedCivs.Random();

                var landSections = landSectionsOfCivs[civ];

                if(TryExpandHomeland(landSections, unassignedSections, partition, mapTemplate)) {
                    int cellsBelongingToCiv = landSections.Sum(section => section.Cells.Count);

                    if(cellsBelongingToCiv >= landCellsPerCiv) {
                        unfinishedCivs.Remove(civ);
                    }
                }else {
                    Debug.LogWarning("Failed to assign new section to civ " + civ.Name);
                    unfinishedCivs.Remove(civ);
                }
            }

            var homelandOfCivs = new Dictionary<ICivilization, CivHomelandData>();

            foreach(var civ in CivFactory.AllCivilizations) {
                var landSections  = landSectionsOfCivs[civ];
                var waterSections = GetCoastForLandSection(landSections, unassignedSections, partition);

                var homelandTemplate = TemplateSelectionLogic.GetHomelandTemplateForCiv(civ, mapTemplate);

                homelandOfCivs[civ] = HomelandGenerator.GetHomelandData(
                    civ, landSections, waterSections, homelandTemplate
                );
            }

            var oceanData = OceanGenerator.GetOceanData(unassignedSections, mapTemplate);

            return new OceanAndContinentData() {
                HomelandDataForCiv = homelandOfCivs,
                OceanData          = oceanData
            };
        }

        private void PaintMap(
            OceanAndContinentData oceansAndContinents, IMapTemplate mapTemplate
        ) {
            foreach(var civ in CivFactory.AllCivilizations) {
                var homeland = oceansAndContinents.HomelandDataForCiv[civ];

                HomelandGenerator.GenerateTopologyAndEcology(homeland);
            }

            OceanGenerator.GenerateTopologyAndEcology(oceansAndContinents.OceanData);

            WaterRationalizer.RationalizeWater(Grid.Cells);

            foreach(var civ in CivFactory.AllCivilizations) {
                var homeland = oceansAndContinents.HomelandDataForCiv[civ];

                HomelandGenerator.DistributeYieldAndResources(homeland);
            }

            OceanGenerator.DistributeYieldAndResources(oceansAndContinents.OceanData);
        }

        private List<MapSection> GetCoastForLandSection(
            List<MapSection> landSections, HashSet<MapSection> unassignedSections, GridPartition partition
        ) {
            var landCells = landSections.SelectMany(section => section.Cells);

            var cellsAdjacentToLand = landCells.SelectMany(cell => Grid.GetNeighbors(cell))
                                               .Distinct()
                                               .Except(landCells);

            var sectionsAdjacentToLand = cellsAdjacentToLand.Select(cell => partition.GetSectionOfCell(cell))
                                                            .Intersect(unassignedSections)
                                                            .Distinct()
                                                            .ToList();

            foreach(var adjacentSection in sectionsAdjacentToLand) {
                unassignedSections.Remove(adjacentSection);
            }

            return sectionsAdjacentToLand;
        }

        private MapSection GetStartingSection(
            HashSet<MapSection> unassignedSections, IEnumerable<MapSection> startingSections,
            IMapTemplate template
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

        private bool TryExpandHomeland(
            List<MapSection> landSections, HashSet<MapSection> unassignedSections,
            GridPartition partition, IMapTemplate template
        ) {
            var expansionCandiates = new HashSet<MapSection>();

            foreach(var section in landSections) {
                foreach(var neighbor in partition.GetNeighbors(section)) {
                    if(unassignedSections.Contains(neighbor)) {
                        expansionCandiates.Add(neighbor);
                    }
                }
            }

            if(expansionCandiates.Any()) {
                var newSection = WeightedRandomSampler<MapSection>.SampleElementsFromSet(
                    expansionCandiates, 1, GetExpansionWeightFunction(
                        landSections, partition, template
                    )
                ).FirstOrDefault();

                if(newSection != null) {
                    unassignedSections.Remove(newSection);
                    landSections.Add(newSection);

                    return true;
                }             
            }

            return false;
        }

        private Func<MapSection, int> GetExpansionWeightFunction(
            List<MapSection> landSections, GridPartition partition,
            IMapTemplate template
        ) {
            return delegate(MapSection region) {
                if(IsWithinHardBorder(region.CentroidCell)) {
                    return 0;
                }

                int borderAvoidanceWeight = GetBorderAvoidanceWeight(region.CentroidCell);

                int neighborsInContinent = partition.GetNeighbors(region).Intersect(landSections).Count();
                int neighborsInContinentWeight = neighborsInContinent * template.NeighborsInContinentWeight;

                int distanceFromSeedCentroid = Grid.GetDistance(landSections[0].CentroidCell, region.CentroidCell);
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

        #endregion
        
    }

}
