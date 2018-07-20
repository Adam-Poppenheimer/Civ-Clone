using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapGeneration {

    public class SubdividingMapGenerator : IHexMapGenerator {

        #region instance fields and properties

        private List<MapRegion> Continents = new List<MapRegion>();
        private List<MapRegion> Oceans     = new List<MapRegion>();



        private IMapGenerationConfig   Config;
        private ICivilizationFactory   CivFactory;
        private ICellModificationLogic ModLogic;
        private IHexGrid               Grid;
        private IContinentGenerator    ContinentGenerator;
        private IOceanGenerator        OceanGenerator;
        private IGridTraversalLogic    GridTraversalLogic;

        #endregion

        #region constructors

        [Inject]
        public SubdividingMapGenerator(
            IMapGenerationConfig config, ICivilizationFactory civFactory,
            ICellModificationLogic modLogic, IHexGrid grid,
            IContinentGenerator continentGenerator,
            IOceanGenerator oceanGenerator, IGridTraversalLogic gridTraversalLogic
        ) {
            Config             = config;
            CivFactory         = civFactory;
            ModLogic           = modLogic;
            Grid               = grid;
            ContinentGenerator = continentGenerator;
            OceanGenerator     = oceanGenerator;
            GridTraversalLogic = gridTraversalLogic;
        }

        #endregion

        #region instance methods

        #region from IHexMapGenerator

        public void GenerateMap(int chunkCountX, int chunkCountZ) {
            var oldRandomState = SetRandomState();

            Continents.Clear();
            Oceans.Clear();

            Grid.Build(chunkCountX, chunkCountZ);

            GenerateCivs();
            GetContinentAndOceanSubdivision_FourContinents();

            FloodMap();

            ContinentGenerator.GenerateContinent(Continents[0], Config.ContinentTemplates.Random());
            ContinentGenerator.GenerateContinent(Continents[1], Config.ContinentTemplates.Random());
            ContinentGenerator.GenerateContinent(Continents[2], Config.ContinentTemplates.Random());
            ContinentGenerator.GenerateContinent(Continents[3], Config.ContinentTemplates.Random());

            foreach(var ocean in Oceans) {
                OceanGenerator.GenerateOcean(ocean);
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

        private void GenerateCivs() {
            foreach(var civ in CivFactory.AllCivilizations.ToArray()) {
                civ.Destroy();
            }

            for(int i = 0; i < Config.CivCount - 1; i++) {
                CivFactory.Create(string.Format("Civ {0}", i + 1), Random.ColorHSV());
            }
        }

        private void GetContinentAndOceanSubdivision() {
            GetContinentAndOceanSubdivision_FourContinents();
        }

        private void GetContinentAndOceanSubdivision_FourContinents() {
            int bottomLeftX  = Grid.CellCountX * 1/4 + Random.Range(-Config.ContinentJitterX, Config.ContinentJitterX); 
            int bottomLeftZ  = Grid.CellCountZ * 2/5 + Random.Range(-Config.ContinentJitterZ, Config.ContinentJitterZ);

            int bottomRightX = Grid.CellCountX * 3/4 + Random.Range(-Config.ContinentJitterX, Config.ContinentJitterX);
            int bottomRightZ = Grid.CellCountZ * 2/5 + Random.Range(-Config.ContinentJitterZ, Config.ContinentJitterZ);

            int topLeftX  = Grid.CellCountX * 1/4 + Random.Range(-Config.ContinentJitterX, Config.ContinentJitterX);
            int topLeftZ  = Grid.CellCountZ * 3/5 + Random.Range(-Config.ContinentJitterZ, Config.ContinentJitterZ);

            int topRightX = Grid.CellCountX * 3/4 + Random.Range(-Config.ContinentJitterX, Config.ContinentJitterX);
            int topRightZ = Grid.CellCountZ * 3/5 + Random.Range(-Config.ContinentJitterZ, Config.ContinentJitterZ);

            var landSeeds = new List<IHexCell>() {
                //Seed in the bottom left quadrant
                Grid.GetCellAtOffset(bottomLeftX, bottomLeftZ),

                //Seed in the bottom right quadrant
                Grid.GetCellAtOffset(bottomRightX, bottomRightZ),

                //Seed in the top left quadrant
                Grid.GetCellAtOffset(topLeftX, topLeftZ),

                //Seed in the top right quadrant
                Grid.GetCellAtOffset(topRightX, topRightZ),
            };

            //Ocean seeds lie between every pair of adjacent land seeds
            var oceanSeeds = new List<IHexCell>() {
                //Oceans between continent seeds
                Grid.GetCellAtOffset((bottomLeftX  + bottomRightX) / 2, (bottomLeftZ  + bottomRightZ) / 2),
                Grid.GetCellAtOffset((bottomLeftX  + topLeftX)     / 2, (bottomLeftZ  + topLeftZ)     / 2),
                Grid.GetCellAtOffset((bottomRightX + topRightX)    / 2, (bottomRightZ + topRightZ)    / 2),
                Grid.GetCellAtOffset((topLeftX     + topRightX)    / 2, (topLeftZ     + topRightZ)    / 2),
            };

            GenerateContinentsAndOceansFromSeeds(landSeeds, oceanSeeds);
        }

        private void GenerateContinentsAndOceansFromSeeds(List<IHexCell> continentSeeds, List<IHexCell> oceanSeeds) {
            var unassignedCells = new HashSet<IHexCell>(Grid.AllCells);

            foreach(var seed in continentSeeds) {
                var newContinent = new MapRegion(Grid);

                newContinent.AddCell(seed);

                Continents.Add(newContinent);
            }

            foreach(var seed in oceanSeeds) {
                var newOcean = new MapRegion(Grid);

                newOcean.AddCell(seed);

                Oceans.Add(newOcean);
            }

            int landCellTarget  = Mathf.RoundToInt(Grid.AllCells.Count * Config.LandPercentage * 0.01f);
            int waterCellTarget = Grid.AllCells.Count - landCellTarget;

            int landCellCount  = Continents.Count;
            int waterCellCount = Oceans.Count;

            var regionEnumerators = new Dictionary<MapRegion, IEnumerator<IHexCell>>();
            foreach(var continent in Continents) {
                regionEnumerators[continent] = GridTraversalLogic.GetCrawlingEnumerator(
                    continent.Seed, unassignedCells, continent.Cells, ContinentWeightFunction
                );
            }

            foreach(var ocean in Oceans) {
                regionEnumerators[ocean] = GridTraversalLogic.GetCrawlingEnumerator(
                    ocean.Seed, unassignedCells, ocean.Cells, OceanWeightFunction
                );
            }

            int iterations = unassignedCells.Count * 10;
            while(unassignedCells.Count > 0 && iterations-- > 0) {
                if(landCellCount < landCellTarget) {
                    foreach(var continent in Continents) {
                        var enumerator = regionEnumerators[continent];
                        if(!enumerator.MoveNext()) {
                            continue;
                        }

                        var nextCell = enumerator.Current;

                        continent.AddCell(nextCell);
                        unassignedCells.Remove(nextCell);
                        if(++landCellCount >= landCellTarget) {
                            break;
                        }
                    }
                }

                if(waterCellCount < waterCellTarget) {
                    foreach(var ocean in Oceans) {
                        var enumerator = regionEnumerators[ocean];
                        if(!enumerator.MoveNext()) {
                            continue;
                        }

                        var nextCell = enumerator.Current;

                        ocean.AddCell(nextCell);
                        unassignedCells.Remove(nextCell);
                        if(++waterCellCount >= waterCellTarget) {
                            break;
                        }
                    }
                }
            }

            AssignOrphanCells(unassignedCells);

            RationalizeOceans();
        }

        private void AssignOrphanCells(HashSet<IHexCell> orphans) {
            var allRegions = Oceans.Concat(Continents);

            var orphanList = new List<IHexCell>(orphans);

            orphanList.Sort(delegate(IHexCell cellOne, IHexCell cellTwo) {
                bool cellOneSurroundedByOrphans = Grid.GetNeighbors(cellOne).All(cell => orphans.Contains(cell));
                bool cellTwoSurroundedByOrphans = Grid.GetNeighbors(cellTwo).All(cell => orphans.Contains(cell));

                if(!cellOneSurroundedByOrphans) {
                    return !cellTwoSurroundedByOrphans ? 0 : -1;
                }else {
                    return !cellTwoSurroundedByOrphans ? 1 : 0;
                }
            });

            foreach(var orphan in orphans) {
                var mostAppropriateRegion = allRegions.Aggregate(delegate(MapRegion currentRegion, MapRegion nextRegion) {
                    var neighbors = Grid.GetNeighbors(orphan);

                    int neighborsInCurrent = neighbors.Where(neighbor => currentRegion.Cells.Contains(neighbor)).Count();
                    int neighborsInNext    = neighbors.Where(neighbor => nextRegion   .Cells.Contains(neighbor)).Count();

                    return neighborsInCurrent >= neighborsInNext ? currentRegion : nextRegion;
                });

                mostAppropriateRegion.AddCell(orphan);
            }
        }

        private void RationalizeOceans() {
            //We set one seed in the middle of the map,
            //one at each of the corners, and one in
            //each of the cardinal directions
            var newOceanSeeds = new List<IHexCell>() {
                //The center
                Grid.GetCellAtOffset(Grid.CellCountX / 2, Grid.CellCountZ / 2),

                //bottom left, bottom right, top left, and top right corners
                Grid.GetCellAtOffset(0,                   0),
                Grid.GetCellAtOffset(0,                   Grid.CellCountZ - 1),
                Grid.GetCellAtOffset(Grid.CellCountX - 1, 0),
                Grid.GetCellAtOffset(Grid.CellCountX - 1, Grid.CellCountZ - 1),

                //north, east, south, and west
                Grid.GetCellAtOffset(Grid.CellCountX / 2, Grid.CellCountZ - 1),
                Grid.GetCellAtOffset(Grid.CellCountX - 1, Grid.CellCountZ / 2),
                Grid.GetCellAtOffset(Grid.CellCountX / 2, 0),
                Grid.GetCellAtOffset(0,                   Grid.CellCountZ / 2),
            };

            var newOceans = new List<MapRegion>();

            foreach(var oceanSeed in newOceanSeeds) {
                var newOcean = new MapRegion(Grid);

                newOcean.AddCell(oceanSeed);

                newOceans.Add(newOcean);
            }

            var oceanCells = new List<IHexCell>();
            foreach(var ocean in Oceans) {
                oceanCells.AddRange(ocean.Cells);
            }

            foreach(var oceanCell in oceanCells) {
                GetNearestOceanToCell(newOceans, oceanCell).AddCell(oceanCell);
            }

            Oceans = newOceans;
        }

        private MapRegion GetNearestOceanToCell(List<MapRegion> oceans, IHexCell cell) {
            MapRegion nearestOcean = null;
            int closestDistance = int.MaxValue;

            foreach(var ocean in oceans) {
                if(nearestOcean == null || Grid.GetDistance(ocean.Seed, cell) < closestDistance) {
                    nearestOcean = ocean;
                    closestDistance = Grid.GetDistance(ocean.Seed, cell);
                }
            }

            return nearestOcean;
        }

        private void FloodMap() {
            foreach(var cell in Grid.AllCells) {
                ModLogic.ChangeTerrainOfCell(cell, CellTerrain.DeepWater);
            }
        }

        private void PaintContinent(MapRegion continent) {
            foreach(var cell in continent.Cells) {
                ModLogic.ChangeTerrainOfCell(cell, CellTerrain.Grassland);
            }
        }

        private void PaintOcean(MapRegion ocean) {
            foreach(var cell in ocean.Cells) {
                ModLogic.ChangeTerrainOfCell(cell, CellTerrain.Snow);
            }
        }

        private IHexCell GetRandomCell() {
            return Grid.AllCells[Random.Range(0, Grid.AllCells.Count)];
        }

        private int ContinentWeightFunction(
            IHexCell cell, IHexCell seed, IEnumerable<IHexCell> acceptedCells
        ) {
            int distanceFromSeed = Grid.GetDistance(seed, cell);
            int jitter = Random.value < Config.JitterProbability ? 1 : 0;

            if(IsWithinHardMapBorder(cell)) {
                return -1;
            }else if(IsWithinSoftMapBorder(cell)) {
                return distanceFromSeed + jitter + Config.SoftBorderAvoidance;
            }else {
                return distanceFromSeed + jitter;
            }
        }

        private int OceanWeightFunction(
            IHexCell cell, IHexCell seed, IEnumerable<IHexCell> acceptedCells
        ) {
            int distanceFromSeed = Grid.GetDistance(seed, cell);
            int selectionBias = Random.value < Config.JitterProbability ? 1 : 0;

            return distanceFromSeed + selectionBias;
        }

        private bool IsWithinHardMapBorder(IHexCell cell) {
            int xOffset = HexCoordinates.ToOffsetCoordinateX(cell.Coordinates);
            int zOffset = HexCoordinates.ToOffsetCoordinateZ(cell.Coordinates);

            return xOffset <= Config.HardMapBorderX || xOffset >= Grid.CellCountX - Config.HardMapBorderX
                || zOffset <= Config.HardMapBorderZ || zOffset >= Grid.CellCountZ - Config.HardMapBorderZ;
        }

        private bool IsWithinSoftMapBorder(IHexCell cell) {
            int xOffset = HexCoordinates.ToOffsetCoordinateX(cell.Coordinates);
            int zOffset = HexCoordinates.ToOffsetCoordinateZ(cell.Coordinates);

            return xOffset <= Config.SoftMapBorderX || xOffset >= Grid.CellCountX - Config.SoftMapBorderX
                || zOffset <= Config.SoftMapBorderZ || zOffset >= Grid.CellCountZ - Config.SoftMapBorderZ;
        }

        #endregion
        
    }

}
