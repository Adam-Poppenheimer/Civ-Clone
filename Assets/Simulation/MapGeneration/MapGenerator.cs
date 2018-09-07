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

            public Dictionary<ICivilization, HomelandData> HomelandDataForCiv;
            public OceanData                                  OceanData;

        }

        #endregion

        #region instance fields and properties

        private IMapGenerationConfig        Config;
        private ICivilizationFactory        CivFactory;
        private IHexGrid                    Grid;
        private IOceanGenerator             OceanGenerator;
        private IStartingUnitPlacementLogic StartingUnitPlacementLogic;
        private IGridPartitionLogic         GridPartitionLogic;
        private IWaterRationalizer          WaterRationalizer;
        private IHomelandGenerator       HomelandGenerator;
        private ITemplateSelectionLogic     TemplateSelectionLogic;
        private ICellClimateLogic           CellClimateLogic;
        private ISectionSubdivisionLogic    SubdivisionLogic;

        #endregion

        #region constructors

        [Inject]
        public MapGenerator(
            IMapGenerationConfig config, ICivilizationFactory civFactory, IHexGrid grid,
            IOceanGenerator oceanGenerator, IGridTraversalLogic gridTraversalLogic,
            IStartingUnitPlacementLogic startingUnitPlacementLogic,
            IGridPartitionLogic gridPartitionLogic, IWaterRationalizer waterRationalizer,
            IHomelandGenerator homelandGenerator, ITemplateSelectionLogic templateSelectionLogic,
            ICellClimateLogic cellClimateLogic, ISectionSubdivisionLogic subdivisionLogic
        ) {
            Config                     = config;
            CivFactory                 = civFactory;
            Grid                       = grid;
            OceanGenerator             = oceanGenerator;
            StartingUnitPlacementLogic = startingUnitPlacementLogic;
            GridPartitionLogic         = gridPartitionLogic;
            WaterRationalizer          = waterRationalizer;
            HomelandGenerator          = homelandGenerator;
            TemplateSelectionLogic     = templateSelectionLogic;
            CellClimateLogic           = cellClimateLogic;
            SubdivisionLogic           = subdivisionLogic;
        }

        #endregion

        #region instance methods

        #region from IHexMapGenerator

        public void GenerateMap(int chunkCountX, int chunkCountZ, IMapTemplate template) {
            CellClimateLogic.Reset(template);

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

            List<List<MapSection>> homelandChunks = SubdivisionLogic.DivideSectionsIntoChunks(
                unassignedSections, partition, CivFactory.AllCivilizations.Count,
                landCellsPerCiv, mapTemplate.MinStartingLocationDistance,
                GetExpansionWeightFunction(partition, mapTemplate), mapTemplate
            );

            var homelandOfCivs = new Dictionary<ICivilization, HomelandData>();

            for(int i = 0; i < CivFactory.AllCivilizations.Count; i++) {
                var civ = CivFactory.AllCivilizations[i];

                var landSections  = homelandChunks[i];
                var waterSections = GetCoastForLandSection(landSections, unassignedSections, partition);

                var homelandTemplate = TemplateSelectionLogic.GetHomelandTemplateForCiv(civ, mapTemplate);

                homelandOfCivs[civ] = HomelandGenerator.GetHomelandData(
                    civ, landSections, waterSections, partition, homelandTemplate, mapTemplate
                );
            }

            var oceanTemplate = mapTemplate.OceanTemplates.Random();

            var oceanData = OceanGenerator.GetOceanData(unassignedSections, oceanTemplate, mapTemplate, partition);

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

                HomelandGenerator.GenerateTopologyAndEcology(homeland, mapTemplate);
            }

            OceanGenerator.GenerateTopologyAndEcology(oceansAndContinents.OceanData);

            WaterRationalizer.RationalizeWater(Grid.Cells);

            foreach(var civ in CivFactory.AllCivilizations) {
                var homeland = oceansAndContinents.HomelandDataForCiv[civ];

                HomelandGenerator.DistributeYieldAndResources(homeland, mapTemplate);
            }

            OceanGenerator.DistributeYieldAndResources(oceansAndContinents.OceanData);
        }

        private List<MapSection> GetCoastForLandSection(
            List<MapSection> landSections, HashSet<MapSection> unassignedSections, GridPartition partition
        ) {
            var unassignedAdjacentToLand = landSections.SelectMany(section => partition.GetNeighbors(section))
                                                       .Intersect(unassignedSections)
                                                       .Distinct()
                                                       .ToList();

            foreach(var adjacentSection in unassignedAdjacentToLand) {
                unassignedSections.Remove(adjacentSection);
            }

            return unassignedAdjacentToLand;
        }

        private ExpansionWeightFunction GetExpansionWeightFunction(
            GridPartition partition, IMapTemplate template
        ) {
            var middleCell = Grid.GetCellAtOffset(Grid.CellCountX / 2, Grid.CellCountZ / 2);

            return delegate(MapSection section, List<MapSection> chunk) {
                if(IsWithinHardBorder(section.CentroidCell, template)) {
                    return 0;
                }

                int borderAvoidanceWeight = GetBorderAvoidanceWeight(section.CentroidCell, template);

                int neighborsInContinent = partition.GetNeighbors(section).Intersect(chunk).Count();
                int neighborsInContinentWeight = neighborsInContinent * template.NeighborsInContinentWeight;

                int distanceFromSeedCentroid = Grid.GetDistance(chunk[0].CentroidCell, section.CentroidCell);
                int distanceFromSeedCentroidWeight = distanceFromSeedCentroid * template.DistanceFromSeedCentroidWeight;

                int distanceFromMapCenter = Grid.GetDistance(section.CentroidCell, middleCell) * template.DistanceFromMapCenterWeight;

                int weight = 1 + Math.Max(
                    0, neighborsInContinentWeight + distanceFromSeedCentroidWeight + borderAvoidanceWeight + distanceFromMapCenter
                );

                return weight;
            };
        }

        private int GetBorderAvoidanceWeight(IHexCell cell, IMapTemplate template) {
            int distanceIntoBorderX = 0, distanceIntoBorderZ = 0;

            var xOffset = HexCoordinates.ToOffsetCoordinateX(cell.Coordinates);
            var zOffset = HexCoordinates.ToOffsetCoordinateZ(cell.Coordinates);

            if(xOffset < template.SoftMapBorderX) {
                distanceIntoBorderX = template.SoftMapBorderX - xOffset;

            }else if(Grid.CellCountX - xOffset < template.SoftMapBorderX) {
                distanceIntoBorderX = template.SoftMapBorderX - (Grid.CellCountX - xOffset);
            }

            if(zOffset < template.SoftMapBorderZ) {
                distanceIntoBorderZ = template.SoftMapBorderZ - zOffset;

            }else if(Grid.CellCountZ - zOffset < template.SoftMapBorderZ) {
                distanceIntoBorderZ = template.SoftMapBorderZ - (Grid.CellCountZ - zOffset);
            }

            return -(distanceIntoBorderX + distanceIntoBorderZ) * template.SoftBorderAvoidanceWeight;
        }

        private bool IsWithinHardBorder(IHexCell cell, IMapTemplate template) {
            var xOffset = HexCoordinates.ToOffsetCoordinateX(cell.Coordinates);
            var zOffset = HexCoordinates.ToOffsetCoordinateZ(cell.Coordinates);

            return xOffset <= template.HardMapBorderX || Grid.CellCountX - xOffset <= template.HardMapBorderX
                || zOffset <= template.HardMapBorderZ || Grid.CellCountZ - zOffset <= template.HardMapBorderZ;
        }

        #endregion
        
    }

}
