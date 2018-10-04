using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public class MapSection {

        #region instance fields and properties

        public IHexCell Seed { get; private set; }

        public ReadOnlyCollection<IHexCell> Cells {
            get { return cells.AsReadOnly(); }
        }
        private List<IHexCell> cells = new List<IHexCell>();

        public int XMin { get; private set; }
        public int XMax { get; private set; }

        public int ZMin { get; private set; }
        public int ZMax { get; private set; }

        public Vector3 Centroid {
            get {
                if(_centroid == null) {
                    if(Cells.Count == 0) {
                        _centroid = Vector3.zero;
                    }else {
                        _centroid = Cells.Select(cell => cell.GridRelativePosition)
                                         .Aggregate((current, next) => current + next) / Cells.Count;
                    }
                }

                return _centroid.GetValueOrDefault();
            }
        }
        private Vector3? _centroid;

        public IHexCell CentroidCell {
            get {
                if(_centroidCell == null) {
                    _centroidCell = Grid.GetCellAtLocation(Centroid);
                }

                return _centroidCell;
            }
        }
        private IHexCell _centroidCell;


        private IHexGrid Grid;

        #endregion

        #region constructors

        public MapSection(IHexGrid grid) {
            Grid   = grid;
        }

        #endregion

        #region instance methods

        #region from Object

        public override string ToString() {
            return string.Format("Map region with centroid {0}", Centroid);
        }

        #endregion

        public int GetDistanceToCell(IHexCell cell) {
            int shorestDistance = int.MaxValue;

            foreach(var cellInRegion in cells) {
                shorestDistance = Math.Min(Grid.GetDistance(cell, cellInRegion), shorestDistance);
            }

            return shorestDistance;
        }

        public IHexCell GetClosestCellToCell(IHexCell targetCell) {
            return cells.Aggregate(delegate(IHexCell current, IHexCell next) {
                int distanceToCurrent = Grid.GetDistance(targetCell, current);
                int distanceToNext    = Grid.GetDistance(targetCell, next);

                return distanceToCurrent <= distanceToNext ? current : next;
            });
        }

        public bool IsCellCompletelyWithin(IHexCell cell) {
            return Grid.GetNeighbors(cell).All(neighbor => cells.Contains(neighbor));
        }

        public bool IsTallerThanWide() {
            return XMax - XMin < ZMax - ZMin;
        }

        public void AddCell(IHexCell newCell) {
            cells.Add(newCell);

            if(Seed == null) {
                Seed = newCell;

                XMin = ZMin = int.MaxValue;
                XMax = ZMax = int.MinValue;
            }

            int xOffset = HexCoordinates.ToOffsetCoordinateX(newCell.Coordinates);
            int zOffset = HexCoordinates.ToOffsetCoordinateZ(newCell.Coordinates);

            XMin = Math.Min(XMin, xOffset);
            XMax = Math.Max(XMax, xOffset);
            ZMin = Math.Min(ZMin, zOffset);
            ZMax = Math.Max(ZMax, zOffset);
        }

        #endregion

    }

}
