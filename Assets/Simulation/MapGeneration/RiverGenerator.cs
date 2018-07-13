using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public class RiverGenerator : IRiverGenerator {

        #region instance fields and properties

        private IHexGrid             Grid;
        private IMapGenerationConfig GenerationConfig;
        private IHexMapConfig        HexMapConfig;
        private IRiverCanon          RiverCanon;

        #endregion

        #region constructors

        [Inject]
        public RiverGenerator(
            IHexGrid grid, IMapGenerationConfig generationConfig,
            IHexMapConfig hexMapConfig, IRiverCanon riverCanon
        ) {
            Grid             = grid;
            GenerationConfig = generationConfig;
            HexMapConfig     = hexMapConfig;
            RiverCanon       = riverCanon;
        }

        #endregion

        #region instance methods

        #region from IRiverGenerator

        public void CreateRivers(List<ClimateData> climateData, int landCellCount) {
            var riverOrigins = new List<IHexCell>();

            int cellCount = Grid.AllCells.Count;
            for(int i = 0; i < cellCount; i++) {
                var cell = Grid.AllCells[i];

                if(cell.Terrain.IsWater()) {
                    continue;
                }

                var climate = climateData[i];

                float weight = (float)(cell.PeakElevation - HexMapConfig.WaterLevel) /
                               (HexMapConfig.MaxElevation - HexMapConfig.WaterLevel);

                weight *= climate.Moisture;

                if (weight > 0.75f) {
				    riverOrigins.Add(cell);
				    riverOrigins.Add(cell);
			    }
			    if (weight > 0.5f) {
				    riverOrigins.Add(cell);
			    }
			    if (weight > 0.25f) {
				    riverOrigins.Add(cell);
			    }
            }

            int riverBudget = Mathf.RoundToInt(landCellCount * GenerationConfig.RiverSegmentPercentage * 0.01f);
            while(riverBudget > 0 && riverOrigins.Count > 0) {
                int index = Random.Range(0, riverOrigins.Count);
                int lastIndex = riverOrigins.Count - 1;

                IHexCell origin = riverOrigins[index];
                riverOrigins[index] = riverOrigins[lastIndex];
                riverOrigins.RemoveAt(lastIndex);

                if(!RiverCanon.HasRiver(origin)) {
                    bool isValidOrigin = true;

                    for(var direction = HexDirection.NE; direction < HexDirection.NW; direction++) {
                        var neighbor = Grid.GetNeighbor(origin, direction);
                        if(neighbor != null && (RiverCanon.HasRiver(neighbor) || neighbor.Terrain.IsWater())) {
                            isValidOrigin = false;
                            break;
                        }
                    }

                    if(isValidOrigin) {
                        riverBudget -= CreateRiver(origin);
                    }
                }
            }
        }

        #endregion

        private int CreateRiver(IHexCell origin) {
            var pathToEndpoint = GetPathToValidEndpoint(origin);

            if(pathToEndpoint == null) {
                return 0;
            }

            CreateRiverStartPoint(pathToEndpoint[0], pathToEndpoint[1]);

            RiverPathResults pathSectionResults = new RiverPathResults(false, false);

            int i = 1;
            for(; i < pathToEndpoint.Count - 1; i++) {
                pathSectionResults = CreateRiverAlongCell(pathToEndpoint[i - 1], pathToEndpoint[i], pathToEndpoint[i + 1]);

                if(!pathSectionResults.Completed) {
                    Debug.LogWarning("Failed to complete river");
                    break;
                }else if(pathSectionResults.FoundWater) {
                    break;
                }
            }

            if(!pathSectionResults.FoundWater) {
                pathSectionResults = CreateRiverEndPoint(pathToEndpoint[i - 1], pathToEndpoint[i]);

                if(!pathSectionResults.Completed) {
                    Debug.LogWarning("Failed to complete river");

                    return i;
                }
            }

            return i + 1;
        }

        private RiverPathResults CreateRiverEndPoint(IHexCell previousCell, IHexCell endingCell) {
            if(endingCell.Terrain.IsWater()) {
                return RiverPathResults.Water;
            }

            var directionToEnd      = GetDirectionOfNeighbor(previousCell, endingCell);
            var directionToPrevious = directionToEnd.Opposite();

            //If the rivers leading out of previousCell are already connected
            //to some river, then we don't need to do anything
            if(RiverCanon.HasRiverAlongEdge(previousCell, directionToEnd)) {
                return RiverPathResults.Success;

            }else if(
                RiverCanon.HasRiverAlongEdge(previousCell, directionToEnd.Next()) &&
                RiverCanon.HasRiverAlongEdge(endingCell, directionToPrevious.Previous())
            ) {
                return RiverPathResults.Success;

            }else if(
                RiverCanon.HasRiverAlongEdge(previousCell, directionToEnd.Previous()) &&
                RiverCanon.HasRiverAlongEdge(endingCell, directionToPrevious.Next())
            ) {
                return RiverPathResults.Success;
            }

            //Case triggers when there's a river along the bottom-left edge of
            //endingCell. We can treat it like a sharp CCW turn towards
            //the cell below and to the left of endingCell
            if(RiverCanon.HasRiverAlongEdge(endingCell, directionToEnd.Previous2())) {
                return CreateRiverAlongCell_SharpCCWTurn(
                    previousCell, endingCell, Grid.GetNeighbor(endingCell, directionToEnd.Previous2()),
                    directionToPrevious, directionToEnd.Previous2()
                );
            }

            //Case triggers when there's a river along the bottom-right edge of
            //endingCell. We can treat it like a sharp CW
            //turn towards the cell below and to the right of endingCell
            if(RiverCanon.HasRiverAlongEdge(endingCell, directionToEnd.Next2())) {
                return CreateRiverAlongCell_SharpCWTurn(
                    previousCell, endingCell, Grid.GetNeighbor(endingCell, directionToEnd.Next2()),
                    directionToPrevious, directionToEnd.Next2()
                );
            }

            //Case triggers when there's a river along the top-left edge of
            //endingCell. In this case, we can treat this like a gentle CCW
            //turn towards the cell above and to the left of endingCell
            if(RiverCanon.HasRiverAlongEdge(endingCell, directionToEnd.Previous())) {
                return CreateRiverAlongCell_GentleCCWTurn(
                    previousCell, endingCell, Grid.GetNeighbor(endingCell, directionToEnd.Previous()),
                    directionToPrevious, directionToEnd.Previous()
                );          
            }

            //Case triggers when there's a river along the top-right edge of
            //endingCell. We can treat it like a gentle CW
            //turn towards the cell above and to the right of endingCell
            if(RiverCanon.HasRiverAlongEdge(endingCell, directionToEnd.Next())) {
                return CreateRiverAlongCell_GentleCWTurn(
                    previousCell, endingCell, Grid.GetNeighbor(endingCell, directionToEnd.Next()),
                    directionToPrevious, directionToEnd.Next()
                );
            }

            //Case triggers when there's a river along the top edge of
            //endingCell. In this case, we can treat this like a
            //straight-across section towards the cell above endingCell
            if(RiverCanon.HasRiverAlongEdge(endingCell, directionToEnd)) {
                return CreateRiverAlongCell_StraightAcross(
                    previousCell, endingCell, Grid.GetNeighbor(endingCell, directionToEnd),
                    directionToPrevious, directionToEnd
                );
            }

            return RiverPathResults.Fail;
        }

        private List<IHexCell> GetPathToValidEndpoint(IHexCell origin) {
            int distance = 0;

            while(distance < GenerationConfig.RiverMaxLengthInHexes) {
                foreach(var cellInRing in Grid.GetCellsInRing(origin, distance++)) {
                    if(cellInRing == origin) {
                        continue;
                    }

                    if(!cellInRing.Terrain.IsWater() && !RiverCanon.HasRiver(cellInRing)) {
                        continue;
                    }

                    var pathToCell = Grid.GetShortestPathBetween(origin, cellInRing, RiverPathCostFunction);
                    if(pathToCell != null) {
                        var retval = new List<IHexCell>() { origin };
                        retval.AddRange(pathToCell);
                        return retval;
                    }
                }
            }
            
            return null;
        }

        private float RiverPathCostFunction(IHexCell currentCell, IHexCell nextCell) {
            if(nextCell.PeakElevation >= currentCell.PeakElevation && nextCell.Shape != CellShape.Flatlands) {
                return -1f;
            }
            
            return 1f;
        }

        //The river should end pointing at NextCell
        private void CreateRiverStartPoint(IHexCell startingCell, IHexCell nextCell) {
            var directionToNeighbor = GetDirectionOfNeighbor(startingCell, nextCell);

            if(Random.value >= 0.5f) {
                if(RiverCanon .CanAddRiverToCell(startingCell, directionToNeighbor.Previous(), RiverFlow.Clockwise)) {
                    RiverCanon.AddRiverToCell   (startingCell, directionToNeighbor.Previous(), RiverFlow.Clockwise);

                }else if(RiverCanon.CanAddRiverToCell(startingCell, directionToNeighbor.Next(), RiverFlow.Counterclockwise)) {
                    RiverCanon     .AddRiverToCell   (startingCell, directionToNeighbor.Next(), RiverFlow.Counterclockwise);

                }else {
                    Debug.LogWarningFormat("Failed to draw a river starting point between cells {0} and {1}", startingCell, nextCell);
                }

            }else {
                if(RiverCanon .CanAddRiverToCell(startingCell, directionToNeighbor.Next(), RiverFlow.Counterclockwise)) {
                    RiverCanon.AddRiverToCell   (startingCell, directionToNeighbor.Next(), RiverFlow.Counterclockwise);

                }else if(RiverCanon.CanAddRiverToCell(startingCell, directionToNeighbor.Previous(), RiverFlow.Clockwise)) {
                    RiverCanon     .AddRiverToCell   (startingCell, directionToNeighbor.Previous(), RiverFlow.Clockwise);

                }else {
                    Debug.LogWarningFormat("Failed to draw a river starting point between cells {0} and {1}", startingCell, nextCell);
                }
            }
        }

        private RiverPathResults CreateRiverAlongCell(IHexCell previousCell, IHexCell currentCell, IHexCell nextCell) {
            var directionToPrevious = GetDirectionOfNeighbor(currentCell, previousCell);
            var directionToNext     = GetDirectionOfNeighbor(currentCell, nextCell);

            if(directionToPrevious == directionToNext.Opposite()) {
                return CreateRiverAlongCell_StraightAcross(previousCell, currentCell, nextCell, directionToPrevious, directionToNext);

            }else if(directionToPrevious == directionToNext.Previous2()) {
                return CreateRiverAlongCell_GentleCCWTurn(previousCell, currentCell, nextCell, directionToPrevious, directionToNext);

            }else if(directionToPrevious == directionToNext.Next2()) {
                return CreateRiverAlongCell_GentleCWTurn(previousCell, currentCell, nextCell, directionToPrevious, directionToNext);

            }else {
                return RiverPathResults.Fail;
            }
        }

        //We need to draw a river from previousCell to nextCell by adding rivers
        //to the edges of currentCell. PreviousCell and nextCell are across from
        //each-other. Previous river should have some river pointing at currentCell
        private RiverPathResults CreateRiverAlongCell_StraightAcross(
            IHexCell previousCell, IHexCell currentCell, IHexCell nextCell,
            HexDirection directionToPrevious, HexDirection directionToNext
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

            return TryFollowSomePath(pathsToTry);
        }

        //We need to draw a river from previousCell to nextCell by adding rivers
        //to the edges of currentCell. PreviousCell is in directionToNext.Previous2(),
        //and should have some river pointing at currentCell
        private RiverPathResults CreateRiverAlongCell_GentleCCWTurn(
            IHexCell previousCell, IHexCell currentCell, IHexCell nextCell,
            HexDirection directionToPrevious, HexDirection directionToNext
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

            return TryFollowSomePath(pathsToTry);
        }

        //We need to draw a river from previousCell to nextCell by adding rivers
        //to the edges of currentCell. PreviousCell is in directionToNext.Previous(),
        //and should have some river pointing at currentCell
        private RiverPathResults CreateRiverAlongCell_SharpCCWTurn(
            IHexCell previousCell, IHexCell currentCell, IHexCell nextCell,
            HexDirection directionToPrevious, HexDirection directionToNext
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

            return TryFollowSomePath(pathsToTry);
        }

        //We need to draw a river from previousCell to nextCell by adding rivers
        //to the edges of currentCell. PreviousCell is in directionToNext.Next2(),
        //and should have some river pointing at currentCell
        private RiverPathResults CreateRiverAlongCell_GentleCWTurn(
            IHexCell previousCell, IHexCell currentCell, IHexCell nextCell,
            HexDirection directionToPrevious, HexDirection directionToNext
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

            return TryFollowSomePath(pathsToTry);
        }

        //We need to draw a river from previousCell to nextCell by adding rivers
        //to the edges of currentCell. PreviousCell is in directionToNext.Next(),
        //and should have some river pointing at currentCell
        private RiverPathResults CreateRiverAlongCell_SharpCWTurn(
            IHexCell previousCell, IHexCell currentCell, IHexCell nextCell,
            HexDirection directionToPrevious, HexDirection directionToNext
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
            if(RiverCanon.HasRiverAlongEdge(previousCell, directionToPrevious.Previous2())) {
                pathsToTry.Add(leftToLeftPath);
                pathsToTry.Add(leftToRightPath);
            }

            //Assuming directionToPrevious.Opposite() is up, 
            //this case triggers when previousCell has a
            //river along its upper right edge
            if(RiverCanon.HasRiverAlongEdge(previousCell, directionToPrevious.Next2())) {
                return RiverPathResults.Success;
            }

            return TryFollowSomePath(pathsToTry);
        }

        private RiverPathResults TryFollowSomePath(List<RiverPath> paths) {
            while(paths.Count > 0) {
                RiverPath randomPath = paths.First();

                var pathResults = randomPath.TryBuildOutPath();

                if(pathResults.Completed) {
                    return pathResults;
                }

                paths.Remove(randomPath);
            }

            return RiverPathResults.Fail;
        }

        private HexDirection GetDirectionOfNeighbor(IHexCell cell, IHexCell neighbor) {
            if(cell == neighbor) {
                throw new System.InvalidOperationException("The argued cells are identical");
            }

            for(var direction = HexDirection.NE; direction <= HexDirection.NW; direction++) {
                if(neighbor == Grid.GetNeighbor(cell, direction)) {
                    return direction;
                }
            }

            throw new System.InvalidOperationException("the argued cells are not neighbors");
        }

        #endregion
        
    }

}
