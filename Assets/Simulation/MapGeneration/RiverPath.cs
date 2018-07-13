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

        public bool CanBuildOutPath() {
            if(Cell.Terrain.IsWater()) {
                return true;
            }

            foreach(var segment in Path) {
                if(RiverCanon.CanAddRiverToCell(Cell, segment, Flow)) {
                    continue;

                }else if(HasWaterInDirection(Cell, segment)){
                    return true;

                }else if(!HasRiverInDirectionWithFlow(Cell, segment, Flow)
                ) {
                    return false;
                }
            }

            return true;
        }

        public RiverPathResults TryBuildOutPath() {
            if(Cell.Terrain.IsWater()) {
                return RiverPathResults.Water;
            }

            foreach(var segment in Path) {
                if(RiverCanon.CanAddRiverToCell(Cell, segment, Flow)) {
                    RiverCanon.AddRiverToCell(Cell, segment, Flow);

                }else if(HasWaterInDirection(Cell, segment)) {
                    return RiverPathResults.Water;

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

        private bool HasWaterInDirection(IHexCell cell, HexDirection direction) {
            return Grid.GetNeighbor(cell, direction).Terrain.IsWater();
        }

        #endregion

    }

}
