using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public class RiverPath {

        #region instance fields and properties

        public IHexCell Cell;

        public List<HexDirection> Path;

        public RiverFlow Flow;



        private IRiverCanon RiverCanon;
        private IHexGrid    Grid;

        #endregion

        #region constructors

        public RiverPath(
            IHexCell cell, HexDirection path,
            RiverFlow flow, IRiverCanon riverCanon, IHexGrid grid
        ) {
            Cell = cell;
            Path = new List<HexDirection>() { path };
            Flow = flow;

            RiverCanon = riverCanon;
            Grid       = grid;
        }

        public RiverPath(
            IHexCell cell, HexDirection pathStart, HexDirection pathEnd,
            RiverFlow flow, IRiverCanon riverCanon, IHexGrid grid
        ) {
            Cell = cell;
            Path = new List<HexDirection>();
            Flow = flow;

            if(flow == RiverFlow.Clockwise) {
                for(HexDirection i = pathStart; i != pathEnd; i = i.Next()) {
                    Path.Add(i);
                }
            }else {
                for(HexDirection i = pathStart; i != pathEnd; i = i.Previous()) {
                    Path.Add(i);
                }
            }

            Path.Add(pathEnd);

            RiverCanon = riverCanon;
            Grid       = grid;
        }

        #endregion

        #region instance methods

        public bool CanBuildOutPath(IEnumerable<IHexCell> oceanCells) {
            if(Cell.Terrain.IsWater()) {
                return true;
            }

            foreach(var segment in Path) {
                if(RiverCanon.CanAddRiverToCell(Cell, segment, Flow)) {
                    continue;

                }else if(HasWaterInDirection(Cell, segment, oceanCells)){
                    return true;

                }else if(!HasRiverInDirectionWithFlow(Cell, segment, Flow)
                ) {
                    return false;
                }
            }

            return true;
        }

        public RiverPathResults TryBuildOutPath(
            HashSet<IHexCell> cellsAdjacentToRiver, IEnumerable<IHexCell> oceanCells
        ) {
            if(Cell.Terrain.IsWater()) {
                return RiverPathResults.Water;
            }

            foreach(var segment in Path) {
                if(HasWaterInDirection(Cell, segment, oceanCells)) {
                    return RiverPathResults.Water;

                } else if(RiverCanon.CanAddRiverToCell(Cell, segment, Flow)) {
                    AddRiverToCell(Cell, segment, Flow, cellsAdjacentToRiver);

                } else if(!HasRiverInDirectionWithFlow(Cell, segment, Flow)){
                    return RiverPathResults.Fail;
                }
            }

            return RiverPathResults.Success;
        }

        private bool HasRiverInDirectionWithFlow(IHexCell cell, HexDirection direction, RiverFlow flow) {
            return (
                RiverCanon.HasRiverAlongEdge   (Cell, direction) && 
                RiverCanon.GetFlowOfRiverAtEdge(Cell, direction) == Flow
            );
        }

        private bool HasWaterInDirection(
            IHexCell cell, HexDirection direction, IEnumerable<IHexCell> oceanCells
        ) {
            var neighbor = Grid.GetNeighbor(cell, direction);
            return neighbor.Terrain.IsWater() || oceanCells.Contains(neighbor);
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

        #endregion

    }

}
