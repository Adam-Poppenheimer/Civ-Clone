using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.HexMap {

    public class RiverCanon : IRiverCanon {

        #region instance fields and properties

        private Dictionary<IHexCell, bool[]> RiverPresenceDict =
            new Dictionary<IHexCell, bool[]>();

        private Dictionary<IHexCell, RiverFlow[]> RiverDirectionDict =
            new Dictionary<IHexCell, RiverFlow[]>();




        private IHexGrid Grid;

        #endregion

        #region constructors

        [Inject]
        public RiverCanon(IHexGrid grid) {
            Grid = grid;
        }

        #endregion

        #region instance methods

        #region from IRiverCanon        

        public bool HasRiver(IHexCell cell) {
            return GetEdgesWithRivers(cell).Count() > 0;
        }

        public bool HasRiverAlongEdge(IHexCell cell, HexDirection edge) {
            return GetPresenceArray(cell)[(int)edge];
        }

        public IEnumerable<HexDirection> GetEdgesWithRivers(IHexCell cell) {
            return EnumUtil.GetValues<HexDirection>().Where(direction => HasRiverAlongEdge(cell, direction));
        }

        public RiverFlow GetFlowOfRiverAtEdge(IHexCell cell, HexDirection edge) {
            if(!HasRiverAlongEdge(cell, edge)) {
                throw new InvalidOperationException("The given cell does not have a river along the given edge");
            }

            return GetDirectionArray(cell)[(int)edge];
        }

        public bool CanAddRiverToCell(IHexCell cell, HexDirection edge, RiverFlow direction) {
            return RiverMeetsPlacementConditions(cell, edge, direction) && !HasRiverAlongEdge(cell, edge);
        }

        public void AddRiverToCell(IHexCell cell, HexDirection edge, RiverFlow direction) {
            if(!CanAddRiverToCell(cell, edge, direction)) {
                throw new InvalidOperationException("CanAddRiverToCell must return true on the given arguments");
            }

            GetPresenceArray (cell)[(int)edge] = true;
            GetDirectionArray(cell)[(int)edge] = direction;

            var neighborAtEdge = Grid.GetNeighbor(cell, edge);

            if(neighborAtEdge != null) {
                GetPresenceArray (neighborAtEdge)[(int)(edge.Opposite())] = true;
                GetDirectionArray(neighborAtEdge)[(int)(edge.Opposite())] = direction.Opposite();
            }

            cell.RefreshSelfOnly();

            foreach(var neighbor in Grid.GetNeighbors(cell)) {
                neighbor.RefreshSelfOnly();
            }
        }

        public void RemoveAllRiversFromCell(IHexCell cell) {
            foreach(var direction in EnumUtil.GetValues<HexDirection>()) {
                var neighborInDirection = Grid.GetNeighbor(cell, direction);

                if(neighborInDirection != null) {
                    RemoveRiverFromCellInDirection(neighborInDirection, direction.Opposite());
                }
            }
        }

        public void RemoveRiverFromCellInDirection(IHexCell cell, HexDirection edge) {
            GetPresenceArray(cell)[(int)edge] = false;

            var neighborInDirection = Grid.GetNeighbor(cell, edge);

            if(neighborInDirection != null) {
                GetPresenceArray(neighborInDirection)[(int)edge.Opposite()] = false;
            }

            cell.RefreshSelfOnly();

            foreach(var neighbor in Grid.GetNeighbors(cell)) {
                neighbor.RefreshSelfOnly();
            }
        }

        public void ValidateRivers(IHexCell cell) {
            foreach(var riveredEdge in GetEdgesWithRivers(cell)) {
                if(!RiverMeetsPlacementConditions(cell, riveredEdge, GetFlowOfRiverAtEdge(cell, riveredEdge))) {
                    RemoveRiverFromCellInDirection(cell, riveredEdge);
                }
            }
        }

        #endregion

        private bool RiverMeetsPlacementConditions(IHexCell cell, HexDirection edge, RiverFlow direction) {
            var neighbor = Grid.GetNeighbor(cell, edge);
            return neighbor != null && !cell.IsUnderwater && !neighbor.IsUnderwater;
        }

        private bool[] GetPresenceArray(IHexCell cell) {
            bool[] retval;

            if(!RiverPresenceDict.ContainsKey(cell)) {
                retval = new bool[6];
                RiverPresenceDict[cell] = retval;
            }else {
                retval = RiverPresenceDict[cell];
            }

            return retval;
        }

        private RiverFlow[] GetDirectionArray(IHexCell cell) {
            RiverFlow[] retval;

            if(!RiverDirectionDict.ContainsKey(cell)) {
                retval = new RiverFlow[6];
                RiverDirectionDict[cell] = retval;
            }else {
                retval = RiverDirectionDict[cell];
            }

            return retval;
        }

        #endregion

    }

}
