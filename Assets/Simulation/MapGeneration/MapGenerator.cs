using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;
using Assets.Simulation.MapResources;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapGeneration {

    public class MapGenerator : IMapGenerator {

        #region instance fields and properties

        private IMapGenerationConfig Config;
        private ICivilizationFactory CivFactory;
        private IHexGrid             Grid;
        private IContinentGenerator  ContinentGenerator;
        private IOceanGenerator      OceanGenerator;
        private IGridTraversalLogic  GridTraversalLogic;
        private IResourceSampler     ResourceSampler;

        #endregion

        #region constructors

        [Inject]
        public MapGenerator(
            IMapGenerationConfig config, ICivilizationFactory civFactory,
            IHexGrid grid, IContinentGenerator continentGenerator,
            IOceanGenerator oceanGenerator, IGridTraversalLogic gridTraversalLogic,
            IResourceSampler resourceSampler
        ) {
            Config              = config;
            CivFactory          = civFactory;
            Grid                = grid;
            ContinentGenerator  = continentGenerator;
            OceanGenerator      = oceanGenerator;
            GridTraversalLogic  = gridTraversalLogic;
            ResourceSampler     = resourceSampler;
        }

        #endregion

        #region instance methods

        #region from IHexMapGenerator

        public void GenerateMap(int chunkCountX, int chunkCountZ, IMapGenerationTemplate template) {
            ResourceSampler.Reset();

            var oldRandomState = SetRandomState();

            Grid.Build(chunkCountX, chunkCountZ);

            GenerateCivs(template.CivCount);

            var unassignedCells = new HashSet<IHexCell>(Grid.AllCells);

            var continents = SplitMapIntoContinents(template, unassignedCells);
            var oceans     = SplitMapIntoOceans    (template, unassignedCells);

            var continentCells = continents.SelectMany(continent => continent.Cells);
            var oceanCells     = oceans    .SelectMany(ocean     => ocean    .Cells);

            for(int i = 0; i < continents.Count; i++) {
                ContinentGenerator.GenerateContinent(
                    continents[i], template.ContinentTemplates.Random(), oceanCells
                );
            }

            foreach(var ocean in oceans) {
                OceanGenerator.GenerateOcean(
                    ocean, template.OceanTemplates.Random(), continentCells
                );
            }

            Random.state = oldRandomState;
        }

        #endregion

        private Random.State SetRandomState() {
            var originalState = Random.state;

            if(!Config.UseFixedSeed) {
                int seed = Random.Range(0, int.MaxValue);
                seed ^= (int)System.DateTime.Now.Ticks;
                seed ^= (int)Time.unscaledTime;
                seed &= int.MaxValue;
                Random.InitState(seed);

                Debug.Log("Using seed " + seed);
            }else {
                Random.InitState(Config.FixedSeed);
            } 

            return originalState;
        }

        private void GenerateCivs(int civCount) {
            foreach(var civ in CivFactory.AllCivilizations.ToArray()) {
                civ.Destroy();
            }

            for(int i = 0; i < civCount; i++) {
                CivFactory.Create(string.Format("Civ {0}", i + 1), Random.ColorHSV());
            }
        }

        private List<MapRegion> SplitMapIntoContinents(
            IMapGenerationTemplate template, HashSet<IHexCell> unassignedCells
        ) {
            var continents = new List<MapRegion>();

            foreach(var section in template.ContinentSections) {
                var newContinent = new MapRegion(Grid);

                DrawContinentContours(
                    newContinent, section, template.ContinentTemplates.Random(), unassignedCells
                );

                continents.Add(newContinent);
            }

            return continents;
        }

        private void DrawContinentContours(
            MapRegion region, MapSection section, IContinentGenerationTemplate template,
            HashSet<IHexCell> unassignedCells
        ) {
            int xOffsetMin, xOffsetMax, zOffsetMin, zOffsetMax;

            CalculateOffsetBounds(
                section, out xOffsetMin, out xOffsetMax, out zOffsetMin, out zOffsetMax
            );

            CrawlingWeightFunction weightFunction = BuildBorderAvoidanceWeightFunction(
                xOffsetMin, xOffsetMax, zOffsetMin, zOffsetMax, template
            );

            var seed = Grid.GetCellAtOffset(
                (xOffsetMin + xOffsetMax) / 2, (zOffsetMin + zOffsetMax) / 2
            );

            var crawler = GridTraversalLogic.GetCrawlingEnumerator(
                seed, unassignedCells, region.Cells, weightFunction
            );

            int cellsInSection = unassignedCells.Where(
                cell => IsCellWithinBounds(cell, xOffsetMin, xOffsetMax, zOffsetMin, zOffsetMax)
            ).Count();

            int desiredLandCount = Mathf.CeilToInt(template.LandPercentage * cellsInSection * 0.01f);

            int iterations = desiredLandCount * 10;
            while(unassignedCells.Any() && iterations-- > 0 && region.Cells.Count < desiredLandCount) {
                if(crawler.MoveNext() && region.Cells.Count < desiredLandCount) {
                    var newCell = crawler.Current;

                    region.AddCell(newCell);

                    unassignedCells.Remove(newCell);
                }
            }
        }

        private CrawlingWeightFunction BuildBorderAvoidanceWeightFunction(
            int xOffsetMinHard, int xOffsetMaxHard, int zOffsetMinHard, int zOffsetMaxHard,
            IContinentGenerationTemplate template
        ) {
            int xOffsetMinSoft = xOffsetMinHard + template.SoftBorderX;
            int xOffsetMaxSoft = xOffsetMaxHard - template.SoftBorderX;

            int zOffsetMinSoft = zOffsetMinHard + template.SoftBorderZ;
            int zOffsetMaxSoft = zOffsetMaxHard - template.SoftBorderZ;

            return delegate(IHexCell cell, IHexCell seed, IEnumerable<IHexCell> acceptedCells) {
                int cellXOffset = HexCoordinates.ToOffsetCoordinateX(cell.Coordinates);
                int cellZOffset = HexCoordinates.ToOffsetCoordinateZ(cell.Coordinates);

                if( cellXOffset < xOffsetMinHard || cellXOffset > xOffsetMaxHard ||
                    cellZOffset < zOffsetMinHard || cellZOffset > zOffsetMaxHard
                ) {
                    return -1;
                }

                int distanceFromSeed = Grid.GetDistance(seed, cell);
                int jitter = Random.value < Config.JitterProbability ? 1 : 0;

                int leftBorderAvoidance  = System.Math.Max(0, xOffsetMinSoft - cellXOffset);
                int rightBorderAvoidance = System.Math.Max(0, cellXOffset - xOffsetMaxSoft);

                int bottomBorderAvoidance  = System.Math.Max(0, zOffsetMinSoft - cellZOffset);
                int topBorderAvoidance     = System.Math.Max(0, cellZOffset - zOffsetMaxSoft);

                return distanceFromSeed + jitter
                     + leftBorderAvoidance   + rightBorderAvoidance
                     + bottomBorderAvoidance + topBorderAvoidance;
            };
        }

        private List<MapRegion> SplitMapIntoOceans(
            IMapGenerationTemplate template, HashSet<IHexCell> unassignedCells
        ) {
            var ocean = new MapRegion(Grid);

            foreach(var cell in unassignedCells) {
                ocean.AddCell(cell);
            }

            unassignedCells.Clear();

            return new List<MapRegion>() { ocean };
        }

        private int ContinentWeightFunction(
            IHexCell cell, IHexCell seed, IEnumerable<IHexCell> acceptedCells
        ) {
            int distanceFromSeed = Grid.GetDistance(seed, cell);
            return distanceFromSeed + (Random.value < Config.JitterProbability ? 1 : 0);
        }

        private int OceanWeightFunction(
            IHexCell cell, IHexCell seed, IEnumerable<IHexCell> acceptedCells
        ) {
            int distanceFromSeed = Grid.GetDistance(seed, cell);
            int selectionBias = Random.value < Config.JitterProbability ? 1 : 0;

            return distanceFromSeed + selectionBias;
        }

        private void CalculateOffsetBounds(
            MapSection section,
            out int offsetXMin, out int offsetXMax,
            out int offsetZMin, out int offsetZMax
        ) {
            offsetXMin = Mathf.RoundToInt((Grid.CellCountX - 1) * section.XMin);
            offsetXMax = Mathf.RoundToInt((Grid.CellCountX - 1) * section.XMax);

            offsetZMin = Mathf.RoundToInt((Grid.CellCountZ - 1) * section.ZMin);
            offsetZMax = Mathf.RoundToInt((Grid.CellCountZ - 1) * section.ZMax);
        }

        private bool IsCellWithinBounds(
            IHexCell cell, int offsetXMin, int offsetXMax,
            int offsetZMin, int offsetZMax
        ) {
            int xOffset = HexCoordinates.ToOffsetCoordinateX(cell.Coordinates);
            int zOffset = HexCoordinates.ToOffsetCoordinateZ(cell.Coordinates);

            return xOffset >= offsetXMin && xOffset <= offsetXMax
                && zOffset >= offsetZMin && zOffset <= offsetZMax;
        }

        #endregion
        
    }

}
