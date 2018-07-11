using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

using UnityCustomUtilities.DataStructures;

namespace Assets.Simulation.MapGeneration {

    public class HexMapGenerator : IHexMapGenerator {

        #region internal types

        private struct MapRegion {
            public int XMin, XMax, ZMin, ZMax;
        }

        #endregion

        #region instance fields and properties

        private int CellCount;

        private List<MapRegion> Regions;


        private IHexGrid               Grid;
        private ICellModificationLogic CellModLogic;
        private IMapGenerationConfig   Config;

        #endregion

        #region constructors

        [Inject]
        public HexMapGenerator(
            IHexGrid grid, ICellModificationLogic cellModLogic,
            IMapGenerationConfig config
        ) {
            Grid         = grid;
            CellModLogic = cellModLogic;
            Config       = config;
        }

        #endregion

        #region instance methods

        #region from IHexMapGenerator

        public void GenerateMap(int chunkCountX, int chunkCountZ) {
            UnityEngine.Random.State originalRandomState = UnityEngine.Random.state;

            if(!Config.UseFixedSeed) {
                int seed = UnityEngine.Random.Range(0, int.MaxValue);
                seed ^= (int)DateTime.Now.Ticks;
                seed ^= (int)Time.unscaledTime;
                seed &= int.MaxValue;
                UnityEngine.Random.InitState(seed);

                Debug.Log("Using seed " + seed);
            }else {
                UnityEngine.Random.InitState(Config.FixedSeed);
            }            

            CellCount = chunkCountX * HexMetrics.ChunkSizeX * chunkCountZ * HexMetrics.ChunkSizeZ;

            Grid.Build(chunkCountX, chunkCountZ);
            CreateRegions();
            CreateLand();
            ReconfigureWater();

            UnityEngine.Random.state = originalRandomState;
        }

        #endregion

        private void CreateRegions() {
            if(Regions == null) {
                Regions = new List<MapRegion>();
            }else {
                Regions.Clear();
            }

            MapRegion region;
            if(Config.RegionCount == 1) {
                region.XMin = Config.MapBorderX;
                region.XMax = Grid.CellCountX - Config.MapBorderX;
                region.ZMin = Config.MapBorderZ;
                region.ZMax = Grid.CellCountZ - Config.MapBorderZ;

                Regions.Add(region);

            } else if(Config.RegionCount == 2) {
                if(UnityEngine.Random.value < 0.5f) {
                    region.XMin = Config.MapBorderX;
                    region.XMax = Grid.CellCountX / 2 - Config.RegionBorder;
                    region.ZMin = Config.MapBorderZ;
                    region.ZMax = Grid.CellCountZ - Config.MapBorderZ;

                    Regions.Add(region);

                    region.XMin = Grid.CellCountX / 2 + Config.RegionBorder;
                    region.XMax = Grid.CellCountX - Config.MapBorderX;

                    Regions.Add(region);
                } else {
                    region.XMin = Config.MapBorderX;
                    region.XMax = Grid.CellCountX - Config.MapBorderX;
                    region.ZMin = Config.MapBorderZ;
                    region.ZMax = Grid.CellCountZ / 2 - Config.RegionBorder;

                    Regions.Add(region);

                    region.ZMin = Grid.CellCountZ / 2 + Config.RegionBorder;
                    region.ZMax = Grid.CellCountZ - Config.MapBorderZ;

                    Regions.Add(region);
                }


            } else if(Config.RegionCount == 3) {
                region.XMin = Config.MapBorderX;
                region.XMax = Grid.CellCountX / 3 - Config.RegionBorder;
                region.ZMin = Config.MapBorderZ;
                region.ZMax = Grid.CellCountZ - Config.MapBorderZ;

                Regions.Add(region);

                region.XMin = Grid.CellCountX / 3 + Config.RegionBorder;
                region.XMax = Grid.CellCountX * 2 / 3 - Config.RegionBorder;

                Regions.Add(region);

                region.XMin = Grid.CellCountX * 2 / 3 + Config.RegionBorder;
                region.XMax = Grid.CellCountX - Config.MapBorderX;

                Regions.Add(region);
            } else {
                region.XMin = Config.MapBorderX;
			    region.XMax = Grid.CellCountX / 2 - Config.RegionBorder;
			    region.ZMin = Config.MapBorderZ;
			    region.ZMax = Grid.CellCountZ / 2 - Config.RegionBorder;

			    Regions.Add(region);

			    region.XMin = Grid.CellCountX / 2 + Config.RegionBorder;
			    region.XMax = Grid.CellCountX - Config.MapBorderX;

			    Regions.Add(region);

			    region.ZMin = Grid.CellCountZ / 2 + Config.RegionBorder;
			    region.ZMax = Grid.CellCountZ - Config.MapBorderZ;

			    Regions.Add(region);

			    region.XMin = Config.MapBorderX;
			    region.XMax = Grid.CellCountX / 2 - Config.RegionBorder;

			    Regions.Add(region);
            }
            
        }

        private void CreateLand() {
            var landCells = new HashSet<IHexCell>();
            var elevationPressures = new Dictionary<IHexCell, float>();

            CalculateLandAndElevation(landCells, elevationPressures);

            ApplyTerrain(landCells);
            ApplyElevation(Grid.AllCells, elevationPressures);
        }

        private void CalculateLandAndElevation(
            HashSet<IHexCell> landCells, Dictionary<IHexCell, float> elevationPressures
        ) {
            int landBudget = Mathf.RoundToInt(CellCount * Config.LandPercentage * 0.01f);
            int iterations = 0;

            int sizeOfSection = UnityEngine.Random.Range(Config.SectionSizeMin, Config.SectionSizeMax + 1);
            while(iterations++ < 10000) {
                foreach(var region in Regions) {
                    landBudget = RaiseLand(sizeOfSection, landBudget, landCells, elevationPressures, region);

                    if(landBudget <= 0) {
                        return;
                    } 
                } 
            }
        }

        private void ApplyElevation(
            IEnumerable<IHexCell> allCells, Dictionary<IHexCell, float> elevationPressures
        ) {
            foreach(var cell in allCells) {
                float elevationPressure = -1f;
                if(elevationPressures.ContainsKey(cell)) {
                    elevationPressure = elevationPressures[cell];
                }

                if(elevationPressure >= Config.MountainThreshold) {
                    CellModLogic.ChangeShapeOfCell(cell, CellShape.Mountains);

                }else if(elevationPressure >= Config.HillsThreshold) {
                    CellModLogic.ChangeShapeOfCell(cell, CellShape.Hills);

                }else if(elevationPressure >= Config.FlatlandsThreshold) {
                    CellModLogic.ChangeShapeOfCell(cell, CellShape.Flatlands);

                }else {
                    CellModLogic.ChangeTerrainOfCell(cell, CellTerrain.DeepWater);
                }
            }
        }

        private void ApplyTerrain(HashSet<IHexCell> landCells) {
            foreach(var cell in landCells) {
                CellModLogic.ChangeTerrainOfCell(cell, CellTerrain.Grassland);
            }
        }

        private int RaiseLand(
            int sizeOfSection, int budget, HashSet<IHexCell> landCells, 
            Dictionary<IHexCell, float> elevationPressures,
            MapRegion region
        ) {
            var startingCell = GetRandomCell(region);

            var frontier = new PriorityQueue<IHexCell>();
            frontier.Add(startingCell, 0);

            int size = 0;
            while(size < sizeOfSection && frontier.Count() > 0) {
                IHexCell current = frontier.DeleteMin();

                if(!landCells.Contains(current)) {
                    landCells.Add(current);
                    elevationPressures[current] = Config.FlatlandsThreshold;

                    if(--budget == 0) {
                        break;
                    }
                }else {
                    elevationPressures[current] += UnityEngine.Random.Range(
                        Config.SectionRaisePressureMin,
                        Config.SectionRaisePressureMax
                    );
                    continue;
                }

                size++;

                foreach(var neighbor in Grid.GetNeighbors(current)) {
                    if(!frontier.Contains(neighbor)) {
                        int distanceFromStart = Grid.GetDistance(startingCell, neighbor);

                        int cellPriority = UnityEngine.Random.value < Config.JitterProbability
                                         ? distanceFromStart + 1 : distanceFromStart;

                        frontier.Add(neighbor, cellPriority);
                    }
                }                
            }

            return budget;
        }

        private void ReconfigureWater() {
            foreach(IEnumerable<IHexCell> bodyOfWater in FindBodiesOfWater(Grid.AllCells)) {
                if(bodyOfWater.Count() <= Config.LakeMaxSize) {
                    foreach(var cell in bodyOfWater) {
                        CellModLogic.ChangeTerrainOfCell(cell, CellTerrain.FreshWater);
                    }

                }else if(bodyOfWater.Count() <= Config.InlandSeaMaxSize) {
                    foreach(var cell in bodyOfWater) {
                        CellModLogic.ChangeTerrainOfCell(cell, CellTerrain.ShallowWater);
                    }

                }else {
                    foreach(var cell in bodyOfWater) {
                        if(HasLandWithinDistance(cell, Config.ContinentalShelfDistance)) {
                            CellModLogic.ChangeTerrainOfCell(cell, CellTerrain.ShallowWater);
                        }
                    }
                }
            }
        }

        private bool HasLandWithinDistance(IHexCell waterCell, int distance) {
            return Grid.GetCellsInRadius(waterCell, distance).Exists(cell => !cell.Terrain.IsWater());
        }

        private List<IEnumerable<IHexCell>> FindBodiesOfWater(IEnumerable<IHexCell> allCells) {
            var retval = new List<IEnumerable<IHexCell>>();

            var unassignedWaterCells = allCells.Where(cell => cell.Terrain.IsWater());

            while(unassignedWaterCells.Any()) {
                var startingCell = unassignedWaterCells.First();
                var bodyOfWater = new HashSet<IHexCell>() { startingCell };

                FindAllWaterConnectedTo(startingCell, bodyOfWater);

                retval.Add(bodyOfWater);
                unassignedWaterCells = unassignedWaterCells.Except(bodyOfWater);
            }

            return retval;
        }

        private void FindAllWaterConnectedTo(IHexCell cell, HashSet<IHexCell> currentBodyOfWater) {
            currentBodyOfWater.Add(cell);

            foreach(var neighbor in Grid.GetNeighbors(cell)) {
                if(neighbor.Terrain.IsWater() && !currentBodyOfWater.Contains(neighbor)) {
                    FindAllWaterConnectedTo(neighbor, currentBodyOfWater);
                }
            }
        }

        private IHexCell GetRandomCell(MapRegion region) {
            return Grid.GetCellAtOffset(
                UnityEngine.Random.Range(region.XMin, region.XMax),
                UnityEngine.Random.Range(region.ZMin, region.ZMax)
            );
        }

        #endregion
        
    }

}
