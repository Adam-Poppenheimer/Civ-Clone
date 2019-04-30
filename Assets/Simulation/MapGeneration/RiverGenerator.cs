using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.Profiling;

using Zenject;

using Assets.Simulation.HexMap;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapGeneration {

    public class RiverGenerator : IRiverGenerator {

        #region instance fields and properties

        private IHexGrid                         Grid;
        private IRiverCanon                      RiverCanon;
        private ICellModificationLogic           ModLogic;
        private IMapGenerationConfig             Config;
        private IHexPathfinder                   HexPathfinder;
        private IWeightedRandomSampler<IHexCell> CellRandomSampler;

        #endregion

        #region constructors

        [Inject]
        public RiverGenerator(
            IHexGrid grid, IRiverCanon riverCanon, ICellModificationLogic modLogic,
            IMapGenerationConfig config, IHexPathfinder hexPathfinder,
            IWeightedRandomSampler<IHexCell> cellRandomSampler

        ) {
            Grid              = grid;
            RiverCanon        = riverCanon;
            ModLogic          = modLogic;
            Config            = config;
            HexPathfinder     = hexPathfinder;
            CellRandomSampler = cellRandomSampler;
        }

        #endregion

        #region instance methods

        #region from IRiverGenerator

        public void CreateRivers(
            IEnumerable<IHexCell> landCells, IEnumerable<IHexCell> waterCells,
            int desiredRiveredCells
        ) {
            Profiler.BeginSample("RiverGenerator.CreateRivers");
            var riveredCells = new HashSet<IHexCell>();

            var riverStartCandidates = landCells.Where(GetRiverStartFilter(waterCells)).ToList();

            HashSet<IHexCell> cellsAdjacentToNewRiver = new HashSet<IHexCell>();

            int iterations = landCells.Count() * 10;
            while(riveredCells.Count < desiredRiveredCells && riverStartCandidates.Count > 0 && iterations-- > 0) {
                var start = CellRandomSampler.SampleElementsFromSet(
                    riverStartCandidates, 1, RiverStartWeightFunction
                ).FirstOrDefault();

                riverStartCandidates.Remove(start);

                Profiler.BeginSample("TryBuildNewRiver");
                if(TryBuildNewRiver(
                    landCells, waterCells, start, desiredRiveredCells - riveredCells.Count,
                    ref cellsAdjacentToNewRiver
                )) {
                    foreach(var cell in cellsAdjacentToNewRiver) {
                        if(ModLogic.CanChangeTerrainOfCell(cell, CellTerrain.FloodPlains)) {
                            ModLogic.ChangeTerrainOfCell(cell, CellTerrain.FloodPlains);
                        }

                        riveredCells.Add(cell);
                        riverStartCandidates.Remove(cell);

                        foreach(var startingNeighbor in Grid.GetNeighbors(start)) {
                            riverStartCandidates.Remove(startingNeighbor);
                        }
                    }
                }
                Profiler.EndSample();
            }
            Profiler.EndSample();
        }

        #endregion

        private List<IHexCell> riverPath = new List<IHexCell>();
        private bool TryBuildNewRiver(
            IEnumerable<IHexCell> landCells, IEnumerable<IHexCell> waterCells,
            IHexCell start, int maxRiveredCellCount, ref HashSet<IHexCell> riveredCells
        ) {
            riveredCells.Clear();

            IHexCell end;

            int maxRiverLength = Math.Max(2, Mathf.RoundToInt(maxRiveredCellCount / 3f));

            if(TryGetRiverEnd(landCells, start, waterCells, maxRiverLength, out end)) {

                riverPath.Clear();

                riverPath.Add(start);

                Profiler.BeginSample("Pathfinding from start to end");
                var weightFunction = BuildRiverWeightFunction(waterCells);

                var pathFrom = HexPathfinder.GetShortestPathBetween(start, end, weightFunction, landCells);
                Profiler.EndSample();

                if(pathFrom == null) {
                    return false;
                }

                riverPath.AddRange(pathFrom);

                CreateRiverStartPoint(riverPath[0], riverPath[1], riveredCells);

                RiverPathResults pathSectionResults = new RiverPathResults(false, false);

                int i = 1;
                for(; i < riverPath.Count - 1; i++) {
                    if(RiverCanon.HasRiver(riverPath[i])) {
                        pathSectionResults = CreateRiverEndpoint(
                            riverPath[i - 1], riverPath[i], waterCells, riveredCells
                        );

                        if(pathSectionResults.Completed) {
                            pathSectionResults.FoundWater = true;
                        }

                        break;

                    }else {
                        pathSectionResults = CreateRiverAlongCell(
                            riverPath[i - 1], riverPath[i], riverPath[i + 1],
                            waterCells, riveredCells
                        );
                    }

                    if(!pathSectionResults.Completed) {
                        Debug.LogWarning("Failed to complete river");
                        break;
                    }else if(pathSectionResults.FoundWater) {
                        break;
                    }
                }

                if(!pathSectionResults.FoundWater) {
                    pathSectionResults = CreateRiverEndpoint(
                        riverPath[i - 1], riverPath[i], waterCells, riveredCells
                    );

                    if(!pathSectionResults.Completed) {
                        Debug.LogWarning("Failed to resolve river endpoint");
                    }
                }

                return true;
            }else {
                return false;
            }
        }

        private bool TryGetRiverEnd(
            IEnumerable<IHexCell> landCells, IHexCell start, IEnumerable<IHexCell> waterCells,
            int maxRiverLength, out IHexCell end
        ) {
            Profiler.BeginSample("TryGetRiverEnd");
            var cellsAdjacentToOcean = landCells.Where(
                cell => Grid.GetNeighbors(cell).Any(neighbor => waterCells.Contains(neighbor) || neighbor.Terrain.IsWater())
            );

            var validEndpoints = cellsAdjacentToOcean.Where(GetRiverEndpointFilter(waterCells, start, maxRiverLength));

            bool retval;

            if(validEndpoints.Any()) {
                end = CellRandomSampler.SampleElementsFromSet(
                    validEndpoints, 1, RiverEndpointWeightFunction
                ).FirstOrDefault();
                retval = end != null;
            }else {
                end = null;
                retval = false;
            }
            Profiler.EndSample();

            return retval;
        }

        private void CreateRiverStartPoint(
            IHexCell startingCell, IHexCell nextCell, HashSet<IHexCell> cellsAdjacentToRiver
        ) {
            var directionToNeighbor = GetDirectionOfNeighbor(startingCell, nextCell);

            if(UnityEngine.Random.value >= 0.5f) {
                if(RiverCanon.CanAddRiverToCell(startingCell, directionToNeighbor.Previous(), RiverFlow.Clockwise)) {
                    AddRiverToCell(startingCell, directionToNeighbor.Previous(), RiverFlow.Clockwise, cellsAdjacentToRiver);

                }else if(RiverCanon.CanAddRiverToCell(startingCell, directionToNeighbor.Next(), RiverFlow.Counterclockwise)) {
                    AddRiverToCell(startingCell, directionToNeighbor.Next(), RiverFlow.Counterclockwise, cellsAdjacentToRiver);

                }else {
                    Debug.LogWarningFormat("Failed to draw a river starting point between cells {0} and {1}", startingCell, nextCell);
                }
            }else {
                if(RiverCanon.CanAddRiverToCell(startingCell, directionToNeighbor.Next(), RiverFlow.Counterclockwise)) {
                    AddRiverToCell(startingCell, directionToNeighbor.Next(), RiverFlow.Counterclockwise, cellsAdjacentToRiver);

                }else if(RiverCanon.CanAddRiverToCell(startingCell, directionToNeighbor.Previous(), RiverFlow.Clockwise)) {
                    AddRiverToCell(startingCell, directionToNeighbor.Previous(), RiverFlow.Clockwise, cellsAdjacentToRiver);

                }else {
                    Debug.LogWarningFormat("Failed to draw a river starting point between cells {0} and {1}", startingCell, nextCell);
                }
            }
        }

        private RiverPathResults CreateRiverAlongCell(
            IHexCell previousCell, IHexCell currentCell, IHexCell nextCell,
            IEnumerable<IHexCell> oceanCells, HashSet<IHexCell> cellsAdjacentToRiver
        ) {
            var directionToPrevious = GetDirectionOfNeighbor(currentCell, previousCell);
            var directionToNext     = GetDirectionOfNeighbor(currentCell, nextCell);

            if(directionToPrevious == directionToNext.Opposite()) {
                return CreateRiverAlongCell_StraightAcross(
                    previousCell, currentCell, nextCell, directionToPrevious,
                    directionToNext, oceanCells, cellsAdjacentToRiver
                );

            }else if(directionToPrevious == directionToNext.Previous2()) {
                return CreateRiverAlongCell_GentleCCWTurn(
                    previousCell, currentCell, nextCell, directionToPrevious,
                    directionToNext, oceanCells, cellsAdjacentToRiver
                );

            }else if(directionToPrevious == directionToNext.Next2()) {
                return CreateRiverAlongCell_GentleCWTurn(
                    previousCell, currentCell, nextCell, directionToPrevious,
                    directionToNext, oceanCells, cellsAdjacentToRiver
                );

            }else {
                return RiverPathResults.Fail;
            }
        }

        private RiverPathResults CreateRiverEndpoint(
            IHexCell previousCell, IHexCell endingCell, IEnumerable<IHexCell> oceanCells,
            HashSet<IHexCell> cellsAdjacentToRiver
        ) {
            //It's possible (though unlikely) that our endpointwd is a water cell. If
            //it is, we don't need to do anything.e
            if(IsWater(endingCell, oceanCells)) {
                return RiverPathResults.Water;
            }

            //If it's not water, we next attempt to connect it to any rivers on the cell
            //or to nearby cells that are submerged in water. We can check and handle
            //both cases at the same time.
            var directionToEnd      = GetDirectionOfNeighbor(previousCell, endingCell);
            var directionToPrevious = directionToEnd.Opposite();

            var bottomLeftNeighbor     = Grid.GetNeighbor(endingCell, directionToEnd.Previous2());
            var topLeftNeighbor        = Grid.GetNeighbor(endingCell, directionToEnd.Previous());
            var straightAcrossNeighbor = Grid.GetNeighbor(endingCell, directionToEnd);
            var topRightNeighbor       = Grid.GetNeighbor(endingCell, directionToEnd.Next());
            var bottomRightNeighbor    = Grid.GetNeighbor(endingCell, directionToEnd.Next2());

            //Case triggers when there's a river or water along the bottom-left edge of
            //endingCell. We can treat it like a sharp CCW turn towards
            //the cell below and to the left of endingCell
            if( RiverCanon.HasRiverAlongEdge(endingCell, directionToEnd.Previous2()) ||
                IsWater(bottomLeftNeighbor, oceanCells)
            ) {
                return CreateRiverAlongCell_SharpCCWTurn(
                    previousCell, endingCell, bottomLeftNeighbor,
                    directionToPrevious, directionToEnd.Previous2(), oceanCells, cellsAdjacentToRiver
                );
            }

            //Case triggers when there's a river or water along the bottom-right edge of
            //endingCell. We can treat it like a sharp CW
            //turn towards the cell below and to the right of endingCell
            if( RiverCanon.HasRiverAlongEdge(endingCell, directionToEnd.Next2()) ||
                IsWater(bottomRightNeighbor, oceanCells)
            ) {
                return CreateRiverAlongCell_SharpCWTurn(
                    previousCell, endingCell, bottomRightNeighbor,
                    directionToPrevious, directionToEnd.Next2(), oceanCells, cellsAdjacentToRiver
                );
            }

            //Case triggers when there's a river or water along the top-left edge of
            //endingCell. In this case, we can treat this like a gentle CCW
            //turn towards the cell above and to the left of endingCell
            if( RiverCanon.HasRiverAlongEdge(endingCell, directionToEnd.Previous()) ||
                IsWater(topLeftNeighbor, oceanCells)
            ) {
                return CreateRiverAlongCell_GentleCCWTurn(
                    previousCell, endingCell, topLeftNeighbor,
                    directionToPrevious, directionToEnd.Previous(), oceanCells, cellsAdjacentToRiver
                );          
            }

            //Case triggers when there's a river or water along the top-right edge of
            //endingCell. We can treat it like a gentle CW
            //turn towards the cell above and to the right of endingCell
            if( RiverCanon.HasRiverAlongEdge(endingCell, directionToEnd.Next()) ||
                IsWater(topRightNeighbor, oceanCells)
            ) {
                return CreateRiverAlongCell_GentleCWTurn(
                    previousCell, endingCell, topRightNeighbor,
                    directionToPrevious, directionToEnd.Next(), oceanCells, cellsAdjacentToRiver
                );
            }

            //Case triggers when there's a river or water along the top edge of
            //endingCell. In this case, we can treat this like a
            //straight-across section towards the cell above endingCell
            if( RiverCanon.HasRiverAlongEdge(endingCell, directionToEnd) ||
                IsWater(straightAcrossNeighbor, oceanCells)
            ) {
                return CreateRiverAlongCell_StraightAcross(
                    previousCell, endingCell, straightAcrossNeighbor,
                    directionToPrevious, directionToEnd, oceanCells, cellsAdjacentToRiver
                );
            }

            return RiverPathResults.Fail;
        }

        //We need to draw a river from previousCell to nextCell by adding rivers
        //to the edges of currentCell. PreviousCell and nextCell are across from
        //each-other. Previous river should have some river pointing at currentCell
        private RiverPathResults CreateRiverAlongCell_StraightAcross(
            IHexCell previousCell, IHexCell currentCell, IHexCell nextCell,
            HexDirection directionToPrevious, HexDirection directionToNext,
            IEnumerable<IHexCell> oceanCells, HashSet<IHexCell> cellsAdjacentToRiver
        ) {
            var leftToLeftPath = new RiverPath(
                currentCell, directionToNext.Previous2(), directionToNext.Previous(),
                RiverFlow.Clockwise, RiverCanon, Grid
            );

            var leftToRightPath = new RiverPath(
                currentCell, directionToPrevious, directionToNext.Next(),
                RiverFlow.Counterclockwise, RiverCanon, Grid
            );

            var rightToRightPath = new RiverPath(
                currentCell, directionToNext.Next2(), directionToNext.Next(),
                RiverFlow.Counterclockwise, RiverCanon, Grid
            );

            var rightToLeftPath = new RiverPath(
                currentCell, directionToPrevious, directionToNext.Previous(),
                RiverFlow.Clockwise, RiverCanon, Grid
            );

            var pathsToTry = new List<RiverPath>();

            //If nextCell is above us, this case happens when previousCell has a
            //river along its left edge
            if(RiverCanon.HasRiverAlongEdge(previousCell, directionToNext.Previous())) {
                pathsToTry.Add(leftToLeftPath);
                pathsToTry.Add(leftToRightPath);

            //If nextCell is above us, this case happens when previousCell has a
            //river along its right edge
            }else if(RiverCanon.HasRiverAlongEdge(previousCell, directionToNext.Next())){
                pathsToTry.Add(rightToRightPath);
                pathsToTry.Add(rightToLeftPath);
            }

            return TryFollowSomePath(pathsToTry, oceanCells, cellsAdjacentToRiver);
        }

        //We need to draw a river from previousCell to nextCell by adding rivers
        //to the edges of currentCell. PreviousCell is in directionToNext.Previous2(),
        //and should have some river pointing at currentCell
        private RiverPathResults CreateRiverAlongCell_GentleCCWTurn(
            IHexCell previousCell, IHexCell currentCell, IHexCell nextCell,
            HexDirection directionToPrevious, HexDirection directionToNext,
            IEnumerable<IHexCell> oceanCells, HashSet<IHexCell> cellsAdjacentToRiver
        ) {
            var leftToLeftPath = new RiverPath(
                currentCell, directionToNext.Previous(),
                RiverFlow.Clockwise, RiverCanon, Grid
            );

            var leftToRightPath = new RiverPath(
                currentCell, directionToPrevious, directionToNext.Next(),
                RiverFlow.Counterclockwise, RiverCanon, Grid
            );

            var rightToRightPath = new RiverPath(
                currentCell, directionToPrevious.Previous(), directionToNext.Next(),
                RiverFlow.Counterclockwise, RiverCanon, Grid
            );

            var rightToLeftPath = new RiverPath(
                currentCell, directionToPrevious, directionToNext.Previous(),
                RiverFlow.Clockwise, RiverCanon, Grid
            );

            var pathsToTry = new List<RiverPath>();

            //If directionToPrevious.Opposite() is up, this case triggers
            //when previousCell has a river along its upper-left edge
            if(RiverCanon.HasRiverAlongEdge(previousCell, directionToNext)) {
                pathsToTry.Add(leftToLeftPath);
                pathsToTry.Add(leftToRightPath);

            //This case triggers when previousCell as a river along its upper-right edge
            }else if(RiverCanon.HasRiverAlongEdge(previousCell, directionToNext.Next2())) {
                pathsToTry.Add(rightToRightPath);
                pathsToTry.Add(rightToLeftPath);
            }

            return TryFollowSomePath(pathsToTry, oceanCells, cellsAdjacentToRiver);
        }

        //We need to draw a river from previousCell to nextCell by adding rivers
        //to the edges of currentCell. PreviousCell is in directionToNext.Previous(),
        //and should have some river pointing at currentCell
        private RiverPathResults CreateRiverAlongCell_SharpCCWTurn(
            IHexCell previousCell, IHexCell currentCell, IHexCell nextCell,
            HexDirection directionToPrevious, HexDirection directionToNext,
            IEnumerable<IHexCell> oceanCells, HashSet<IHexCell> cellsAdjacentToRiver
        ) {
            var rightToRightPath = new RiverPath(
                currentCell, directionToPrevious.Previous(), directionToNext.Next(),
                RiverFlow.Counterclockwise, RiverCanon, Grid
            );

            var rightToLeftPath = new RiverPath(
                currentCell, directionToPrevious, RiverFlow.Clockwise,
                RiverCanon, Grid
            );

            var pathsToTry = new List<RiverPath>();

            //Assuming directionToPrevious.Opposite() is up,
            //this case triggers when previousCell has a
            //river along its upper left edge
            if(RiverCanon.HasRiverAlongEdge(previousCell, directionToPrevious.Next2())) {
                return RiverPathResults.Success;
            }

            //this case triggers when previousCell has a
            //river along its upper right edge
            if(RiverCanon.HasRiverAlongEdge(previousCell, directionToPrevious.Previous2())) {
                pathsToTry.Add(rightToRightPath);
                pathsToTry.Add(rightToLeftPath);                
            }

            return TryFollowSomePath(pathsToTry, oceanCells, cellsAdjacentToRiver);
        }

        //We need to draw a river from previousCell to nextCell by adding rivers
        //to the edges of currentCell. PreviousCell is in directionToNext.Next2(),
        //and should have some river pointing at currentCell
        private RiverPathResults CreateRiverAlongCell_GentleCWTurn(
            IHexCell previousCell, IHexCell currentCell, IHexCell nextCell,
            HexDirection directionToPrevious, HexDirection directionToNext,
            IEnumerable<IHexCell> oceanCells, HashSet<IHexCell> cellsAdjacentToRiver
        ) {
            var leftLeftPath = new RiverPath(
                currentCell, directionToPrevious.Next(), directionToNext.Previous(),
                RiverFlow.Clockwise, RiverCanon, Grid
            );

            var leftRightPath = new RiverPath(
                currentCell, directionToPrevious, directionToNext.Next(),
                RiverFlow.Counterclockwise, RiverCanon, Grid
            );

            var rightRightPath = new RiverPath(
                currentCell, directionToNext.Next(),
                RiverFlow.Counterclockwise, RiverCanon, Grid
            );

            var rightleftPath = new RiverPath(
                currentCell, directionToPrevious, directionToNext.Previous(),
                RiverFlow.Clockwise, RiverCanon, Grid
            );

            var pathsToTry = new List<RiverPath>();

            //If directionToNext.Previous() is considered up,
            //this case triggers when previousCell has a river
            //along its upper-left edge.
            if(RiverCanon.HasRiverAlongEdge(previousCell, directionToPrevious.Next2())) {
                pathsToTry.Add(leftLeftPath);
                pathsToTry.Add(leftRightPath);
            }

            //If directionToNext.Previous() is considered up,
            //this case triggers when previousCell has a river
            //along its upper-right edge.
            if(RiverCanon.HasRiverAlongEdge(previousCell, directionToPrevious.Previous2())) {
                pathsToTry.Add(rightRightPath);
                pathsToTry.Add(rightleftPath);
            }

            return TryFollowSomePath(pathsToTry, oceanCells, cellsAdjacentToRiver);
        }

        //We need to draw a river from previousCell to nextCell by adding rivers
        //to the edges of currentCell. PreviousCell is in directionToNext.Next(),
        //and should have some river pointing at currentCell
        private RiverPathResults CreateRiverAlongCell_SharpCWTurn(
            IHexCell previousCell, IHexCell currentCell, IHexCell nextCell,
            HexDirection directionToPrevious, HexDirection directionToNext,
            IEnumerable<IHexCell> oceanCells, HashSet<IHexCell> cellsAdjacentToRiver
        ) {
            var leftToLeftPath = new RiverPath(
                currentCell, directionToPrevious.Next(), directionToNext.Previous(),
                RiverFlow.Clockwise, RiverCanon, Grid
            );

            var leftToRightPath = new RiverPath(
                currentCell, directionToPrevious, RiverFlow.Counterclockwise,
                RiverCanon, Grid
            );

            var pathsToTry = new List<RiverPath>();

            //Assuming directionToPrevious.Opposite() is up, 
            //this case triggers when previousCell has a
            //river along its upper left edge
            if(RiverCanon.HasRiverAlongEdge(previousCell, directionToPrevious.Next2())) {
                pathsToTry.Add(leftToLeftPath);
                pathsToTry.Add(leftToRightPath);
            }

            //Assuming directionToPrevious.Opposite() is up, 
            //this case triggers when previousCell has a
            //river along its upper right edge
            if(RiverCanon.HasRiverAlongEdge(previousCell, directionToPrevious.Previous2())) {
                return RiverPathResults.Success;
            }

            return TryFollowSomePath(pathsToTry, oceanCells, cellsAdjacentToRiver);
        }

        private RiverPathResults TryFollowSomePath(
            List<RiverPath> paths, IEnumerable<IHexCell> oceanCells, HashSet<IHexCell> cellsAdjacentToRiver
        ) {
            if(paths.Any(path => path.IsAlreadyCompleted())) {
                return RiverPathResults.Success;
            }

            while(paths.Count > 0) {
                RiverPath randomPath = paths.First();

                var pathResults = randomPath.TryBuildOutPath(cellsAdjacentToRiver, oceanCells);

                if(pathResults.Completed) {
                    return pathResults;
                }

                paths.Remove(randomPath);
            }

            return RiverPathResults.Fail;
        }

        private HexDirection GetDirectionOfNeighbor(IHexCell cell, IHexCell neighbor) {
            if(cell == neighbor) {
                throw new InvalidOperationException("The argued cells are identical");
            }

            for(var direction = HexDirection.NE; direction <= HexDirection.NW; direction++) {
                if(neighbor == Grid.GetNeighbor(cell, direction)) {
                    return direction;
                }
            }

            throw new InvalidOperationException("the argued cells are not neighbors");
        }

        private void AddRiverToCell(
            IHexCell cell, HexDirection direction, RiverFlow flow, HashSet<IHexCell> cellsAdjacentToRiver
        ) {
            RiverCanon.AddRiverToCell(cell, direction, flow);

            cellsAdjacentToRiver.Add(cell);

            if(Grid.HasNeighbor(cell, direction)) {
                cellsAdjacentToRiver.Add(Grid.GetNeighbor(cell, direction));
            }
        }

        private bool IsWater(IHexCell cell, IEnumerable<IHexCell> oceanCells) {
            return cell != null && cell.Terrain.IsWater() || oceanCells.Contains(cell);
        }

        private Func<IHexCell, bool> GetRiverStartFilter(IEnumerable<IHexCell> waterCells) {
            return delegate(IHexCell cell) {
                return cell.Shape != CellShape.Flatlands
                    && !Grid.GetNeighbors(cell).Any(neighbor => IsWater(neighbor, waterCells));
            };            
        }

        private int RiverStartWeightFunction(IHexCell cell) {
            int nearbyDesert = Grid.GetCellsInRadius(cell, 1).Count(nearby => nearby.Terrain == CellTerrain.Desert);
            int nearbyArctic = Grid.GetCellsInRadius(cell, 1).Count(nearby => nearby.Terrain.IsArctic());

            return 1 + nearbyDesert * Config.RiverEndpointOnDesertWeight + nearbyArctic * Config.RiverEndpointOnArcticWeight;
        }

        private Func<IHexCell, bool> GetRiverEndpointFilter(
            IEnumerable<IHexCell> waterCells, IHexCell start, int maxRiverLength
        ) {
            return delegate(IHexCell cell) {
                return cell != start
                    && !IsWater(cell, waterCells)
                    && !RiverCanon.HasRiver(cell)
                    && Grid.GetDistance(cell, start) <= maxRiverLength;
            };
        }

        private int RiverEndpointWeightFunction(IHexCell cell) {
            int nearbyDesert = Grid.GetCellsInRadius(cell, 1).Count(nearby => nearby.Terrain == CellTerrain.Desert);
            int nearbyArctic = Grid.GetCellsInRadius(cell, 1).Count(nearby => nearby.Terrain.IsArctic());

            return 1 + nearbyDesert * Config.RiverEndpointOnDesertWeight + nearbyArctic * Config.RiverEndpointOnArcticWeight;
        }

        private Func<IHexCell, IHexCell, float> BuildRiverWeightFunction(IEnumerable<IHexCell> oceanCells) {
            return delegate(IHexCell currentCell, IHexCell nextCell) {
                if(IsWater(nextCell, oceanCells)) {
                    return -1f;
                }else if(Grid.GetNeighbors(nextCell).Any(neighbor => IsWater(neighbor, oceanCells))) {
                    return 2f;
                }else {
                    return 1f;
                }
            };
        }

        #endregion
        
    }

}
