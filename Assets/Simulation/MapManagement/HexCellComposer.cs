using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Barbarians;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapManagement {

    public class HexCellComposer : IHexCellComposer {

        #region static fields and properties

        private static HexDirection[] RiverDirections = new HexDirection[] {
            HexDirection.NE, HexDirection.E, HexDirection.SE
        };

        #endregion

        #region instance fields and properties

        private IHexGrid               Grid;
        private IRiverCanon            RiverCanon;
        private ICellModificationLogic CellModificationLogic;

        #endregion

        #region constructors

        [Inject]
        public HexCellComposer(
            IHexGrid grid, IRiverCanon riverCanon, ICellModificationLogic cellModificationLogic
        ) {
            Grid                  = grid;
            RiverCanon            = riverCanon;
            CellModificationLogic = cellModificationLogic;
        }

        #endregion

        #region instance methods

        public void ClearRuntime() {
            Grid.Clear();
        }

        public void ComposeCells(SerializableMapData mapData) {
            mapData.CellCountX = Grid.CellCountX;
            mapData.CellCountZ = Grid.CellCountZ;

            mapData.HexCells = new List<SerializableHexCellData>();

            foreach(var cell in Grid.Cells) {
                var newCellData = new SerializableHexCellData() {
                    Coordinates            = cell.Coordinates,
                    Terrain                = cell.Terrain,
                    Shape                  = cell.Shape,
                    Vegetation             = cell.Vegetation,
                    Feature                = cell.Feature,
                    SuppressSlot           = cell.SuppressSlot,
                    HasRoads               = cell.HasRoads,
                    IsSlotOccupied         = cell.WorkerSlot.IsOccupied,
                    IsSlotLocked           = cell.WorkerSlot.IsLocked,
                    HasRiverAtEdge         = new List<bool>(new bool[6]),
                    DirectionOfRiverAtEdge = new List<RiverFlow>(new RiverFlow[6])
                };

                //We only need to compose rivers on three sides of the cell.
                //The other three will be handled by the cell's neighbors.
                //It's guaranteed to have a neighbor in any rivered direction
                //because rivers cannot be placed along an edge without two
                //living neighbors.
                foreach(var edgeWithRiver in RiverCanon.GetEdgesWithRivers(cell).Intersect(RiverDirections)) {
                    newCellData.HasRiverAtEdge[(int)edgeWithRiver] = true;

                    newCellData.DirectionOfRiverAtEdge[(int)edgeWithRiver]
                        = RiverCanon.GetFlowOfRiverAtEdge(cell, edgeWithRiver);
                }

                mapData.HexCells.Add(newCellData);
            }
        }

        public void DecomposeCells(SerializableMapData mapData) {
            Grid.Build(mapData.CellCountX, mapData.CellCountZ);

            foreach(var cellData in mapData.HexCells) {
                var cellToModify = Grid.GetCellAtCoordinates(cellData.Coordinates);

                CellModificationLogic.ChangeTerrainOfCell   (cellToModify, cellData.Terrain);
                CellModificationLogic.ChangeShapeOfCell     (cellToModify, cellData.Shape);
                CellModificationLogic.ChangeVegetationOfCell(cellToModify, cellData.Vegetation);
                CellModificationLogic.ChangeFeatureOfCell   (cellToModify, cellData.Feature);
                CellModificationLogic.ChangeHasRoadsOfCell  (cellToModify, cellData.HasRoads);

                cellToModify.SuppressSlot = cellData.SuppressSlot;

                //Converging rivers (where two rivers combine and flow into a third) have
                //order-sensitive creation, since attempting to place both of the inflow
                //rivers before the outflow river has been created is invalid. To account for
                //this, we delay the creation of any invalid rivers (since those represent
                //an inflow being attached to another inflow) until after all other rivers
                //have been placed.  
                var delayedRivers = new List<Tuple<IHexCell, HexDirection, RiverFlow>>();

                for(int i = 0; i < 6; i++) {
                    var edge = (HexDirection)i;

                    if(cellData.HasRiverAtEdge[i] && !RiverCanon.HasRiverAlongEdge(cellToModify, edge)) {

                        if(RiverCanon.CanAddRiverToCell(cellToModify, edge, cellData.DirectionOfRiverAtEdge[i])) {
                            RiverCanon.AddRiverToCell(cellToModify, edge, cellData.DirectionOfRiverAtEdge[i]);

                        }else {
                            delayedRivers.Add(new Tuple<IHexCell, HexDirection, RiverFlow>(
                                cellToModify, edge, cellData.DirectionOfRiverAtEdge[i]
                            ));
                        }

                    }
                }

                foreach(var river in delayedRivers) {
                    if(RiverCanon.CanAddRiverToCell(river.Item1, river.Item2, river.Item3)) {
                        RiverCanon.AddRiverToCell(river.Item1, river.Item2, river.Item3);
                    }else {
                        throw new InvalidOperationException(string.Format(
                            "Failed to decompose river ({0}, {1}, {2})",
                            river.Item1, river.Item2, river.Item3
                        ));
                    }                    
                }

                cellToModify.WorkerSlot.IsOccupied = cellData.IsSlotOccupied;
                cellToModify.WorkerSlot.IsLocked   = cellData.IsSlotLocked;
            }
        }

        #endregion

    }

}
