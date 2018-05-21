using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapManagement {

    public class HexCellComposer : IHexCellComposer {

        #region instance fields and properties

        private IHexGrid    Grid;
        private IRiverCanon RiverCanon;

        #endregion

        #region constructors

        [Inject]
        public HexCellComposer(IHexGrid grid, IRiverCanon riverCanon) {
            Grid       = grid;
            RiverCanon = riverCanon;
        }

        #endregion

        #region instance methods

        public void ClearRuntime() {
            Grid.Clear();
        }

        public void ComposeCells(SerializableMapData mapData) {
            mapData.HexCells = new List<SerializableHexCellData>();

            foreach(var cell in Grid.AllCells) {
                var newCellData = new SerializableHexCellData() {
                    Coordinates            = cell.Coordinates,
                    Terrain                = cell.Terrain,
                    Feature                = cell.Feature,
                    Shape                  = cell.Shape,
                    SuppressSlot           = cell.SuppressSlot,
                    HasRoads               = cell.HasRoads,
                    IsSlotOccupied         = cell.WorkerSlot.IsOccupied,
                    IsSlotLocked           = cell.WorkerSlot.IsLocked,
                    HasRiverAtEdge         = new bool[6],
                    DirectionOfRiverAtEdge = new RiverDirection[6]
                };

                foreach(var edgeWithRiver in RiverCanon.GetEdgesWithRivers(cell)) {
                    newCellData.HasRiverAtEdge[(int)edgeWithRiver] = true;

                    newCellData.DirectionOfRiverAtEdge[(int)edgeWithRiver]
                        = RiverCanon.GetFlowDirectionOfRiverAtEdge(cell, edgeWithRiver);
                }

                mapData.HexCells.Add(newCellData);
            }
        }

        public void DecomposeCells(SerializableMapData mapData) {
            Grid.Build();

            foreach(var cellData in mapData.HexCells) {
                var cellToModify = Grid.GetCellAtCoordinates(cellData.Coordinates);

                cellToModify.Terrain             = cellData.Terrain;
                cellToModify.Feature             = cellData.Feature;
                cellToModify.Shape               = cellData.Shape;
                cellToModify.SuppressSlot        = cellData.SuppressSlot;
                cellToModify.HasRoads            = cellData.HasRoads;

                for(int i = 0; i < 6; i++) {
                    var edge = (HexDirection)i;

                    if(cellData.HasRiverAtEdge[i] && !RiverCanon.HasRiverAlongEdge(cellToModify, edge)) {
                        RiverCanon.AddRiverToCell(cellToModify, edge, cellData.DirectionOfRiverAtEdge[i]);
                    }
                }

                cellToModify.WorkerSlot.IsOccupied = cellData.IsSlotOccupied;
                cellToModify.WorkerSlot.IsLocked   = cellData.IsSlotLocked;
            }
        }

        #endregion

    }

}
