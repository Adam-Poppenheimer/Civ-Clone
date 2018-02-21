﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapManagement {

    public class HexCellComposer {

        #region instance fields and properties

        private IHexGrid Grid;

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
                    Coordinates = cell.Coordinates,
                    Terrain      = cell.Terrain,
                    Feature      = cell.Feature,
                    Elevation    = cell.Elevation,
                    WaterLevel   = cell.WaterLevel,
                    SuppressSlot = cell.SuppressSlot,

                    Roads = EnumUtil.GetValues<HexDirection>().Select(
                        direction => cell.HasRoadThroughEdge(direction)
                    ).ToList(),

                    HasOutgoingRiver = RiverCanon.HasOutgoingRiver(cell),
                    OutgoingRiver    = RiverCanon.GetOutgoingRiver(cell),

                    IsSlotOccupied = cell.WorkerSlot.IsOccupied,
                    IsSlotLocked   = cell.WorkerSlot.IsLocked
                };

                mapData.HexCells.Add(newCellData);
            }
        }

        public void DecomposeCells(SerializableMapData mapData) {
            Grid.Build();

            foreach(var cellData in mapData.HexCells) {
                var cellToModify = Grid.GetCellAtCoordinates(cellData.Coordinates);

                cellToModify.Terrain      = cellData.Terrain;
                cellToModify.Feature      = cellData.Feature;
                cellToModify.Elevation    = cellData.Elevation;
                cellToModify.WaterLevel   = cellData.WaterLevel;
                cellToModify.SuppressSlot = cellData.SuppressSlot;

                foreach(var direction in EnumUtil.GetValues<HexDirection>()) {
                    if(cellData.Roads[(int)direction]) {
                        cellToModify.AddRoad(direction);
                    }
                }

                if(cellData.HasOutgoingRiver) {
                    RiverCanon.SetOutgoingRiver(cellToModify, cellData.OutgoingRiver);
                }

                cellToModify.WorkerSlot.IsOccupied = cellData.IsSlotOccupied;
                cellToModify.WorkerSlot.IsLocked   = cellData.IsSlotLocked;
            }
        }

        #endregion

    }

}
