﻿using System;
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
        private IResourceDistributor   ResourceDistributor;
        private IMapGenerationConfig   Config;
        private ICellClimateLogic      CellClimateLogic;

        #endregion

        #region constructors

        [Inject]
        public RegionGenerator(
            ICellModificationLogic modLogic, IHexGrid grid,
            IResourceDistributor resourceDistributor,
            IMapGenerationConfig config, ICellClimateLogic cellClimateLogic
        ) {
            ModLogic            = modLogic;
            Grid                = grid;
            ResourceDistributor = resourceDistributor;
            Config              = config;
            CellClimateLogic    = cellClimateLogic;
        }

        #endregion

        #region instance methods

        #region from IRegionGenerator

        public void GenerateTopology(MapRegion region, IRegionTopologyTemplate template) {
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

        public void PaintTerrain(MapRegion region, IRegionBiomeTemplate template) {
            var unassignedLandCells = new HashSet<IHexCell>(region.LandCells);

            PaintArcticTerrain(region, template, unassignedLandCells);
            PaintOtherTerrains(region, template, unassignedLandCells);
            AssignLandOrphans (region, template, unassignedLandCells);

            foreach(var cell in region.WaterCells) {
                ModLogic.ChangeTerrainOfCell(cell, CellTerrain.ShallowWater);
            }
        }

        public void AssignFloodPlains(IEnumerable<IHexCell> landCells) {
            foreach(var desertCell in landCells.Where(cell => cell.Terrain == CellTerrain.Desert)) {
                if(ModLogic.CanChangeTerrainOfCell(desertCell, CellTerrain.FloodPlains)) {
                    ModLogic.ChangeTerrainOfCell(desertCell, CellTerrain.FloodPlains);
                }
            }
        }

        public void DistributeYieldAndResources(MapRegion region, RegionData regionData) {
            ResourceDistributor.DistributeStrategicResourcesAcrossRegion(region, regionData);
        }

        #endregion

        private void PaintOtherTerrains(
            MapRegion region, IRegionBiomeTemplate template, HashSet<IHexCell> unassignedLandCells
        ) {
            var terrainsToPaint = new List<CellTerrain>() {
                CellTerrain.Grassland, CellTerrain.Plains, CellTerrain.Desert
            };

            var percentageOfTerrains  = new Dictionary<CellTerrain, int>() {
                { CellTerrain.Grassland, template.GrasslandPercentage },
                { CellTerrain.Plains,    template.PlainsPercentage    },
                { CellTerrain.Desert,    template.DesertPercentage    },
            };

            int landToPaintCount = unassignedLandCells.Count;

            foreach(var terrain in terrainsToPaint) {
                var weightFunction = GetWeightFunction(terrain);

                int terrainCount = Mathf.RoundToInt(percentageOfTerrains[terrain] * 0.01f * landToPaintCount);
                terrainCount = Math.Min(terrainCount, unassignedLandCells.Count);

                var changeCandidatesDescending = new List<IHexCell>(unassignedLandCells);

                changeCandidatesDescending.Sort((first, second) => weightFunction(second).CompareTo(weightFunction(first)));

                for(int i = 0; i < terrainCount; i++) {
                    var cellToChange = changeCandidatesDescending[i];

                    ModLogic.ChangeTerrainOfCell(cellToChange, terrain);
                    unassignedLandCells.Remove(cellToChange);
                }
            }
        }

        private void PaintArcticTerrain(
            MapRegion region, IRegionBiomeTemplate template, HashSet<IHexCell> unassignedLandCells
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
            MapRegion region, IRegionBiomeTemplate template, HashSet<IHexCell> unassignedLandCells
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

        private int HillsStartingWeightFunction(IHexCell cell) {
            int weight = UnityEngine.Random.Range(2, 3);

            if(Grid.GetNeighbors(cell).Exists(neighbor => neighbor.Terrain.IsWater())) {
                weight -= 1;
            }

            return weight;
        }

        private int HillsDynamicWeightFunction(IHexCell cell, List<IHexCell> elevatedCells) {
            int adjacentHillCount = Grid.GetNeighbors(cell).Where(adjacent => elevatedCells.Contains(adjacent)).Count();

            return 2 + adjacentHillCount < 2 ? (10 * adjacentHillCount) : (5 - adjacentHillCount);
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

        private Func<IHexCell, int> GetWeightFunction(CellTerrain terrain) {
            float idealTemperature   = Config.GetIdealTemperatureForTerrain  (terrain);
            float idealPrecipitation = Config.GetIdealPrecipitationForTerrain(terrain);

            return delegate(IHexCell cell) {
                if(!ModLogic.CanChangeTerrainOfCell(cell, terrain)) {
                    return 0;
                }else {
                    float cellTemperature   = CellClimateLogic.GetTemperatureOfCell  (cell);
                    float cellPrecipitation = CellClimateLogic.GetPrecipitationOfCell(cell);

                    int weight = Config.BaseTerrainWeight -
                                 Config.TerrainTemperatureWeight   * Mathf.RoundToInt(Mathf.Abs(idealTemperature   - cellTemperature)) -
                                 Config.TerrainPrecipitationWeight * Mathf.RoundToInt(Mathf.Abs(idealPrecipitation - cellPrecipitation));

                    return Math.Max(weight, 0);
                }
            };
        }

        #endregion
        
    }

}
