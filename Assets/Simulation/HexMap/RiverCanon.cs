using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.HexMap {

    public class RiverCanon : IRiverCanon {

        #region instance fields and properties

        private Dictionary<IHexCell, bool[]> RiverPresenceDict =
            new Dictionary<IHexCell, bool[]>();

        private Dictionary<IHexCell, RiverFlow[]> RiverDirectionDict =
            new Dictionary<IHexCell, RiverFlow[]>();




        private IHexGrid                  Grid;
        private IRiverCornerValidityLogic RiverCornerValidityLogic;

        #endregion

        #region constructors

        [Inject]
        public RiverCanon(
            IHexGrid grid, IRiverCornerValidityLogic riverCornerValidityLogic, HexCellSignals cellSignals
        ) {
            Grid                     = grid;
            RiverCornerValidityLogic = riverCornerValidityLogic;

            cellSignals.MapBeingClearedSignal.Subscribe(unit => Clear());
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

        public bool CanAddRiverToCell(IHexCell cell, HexDirection edge, RiverFlow flow) {
            if(HasRiverAlongEdge(cell, edge)) {
                return false;
            }

            return RiverMeetsPlacementConditions(cell, edge, flow);
        }

        public void AddRiverToCell(IHexCell cell, HexDirection edge, RiverFlow flow) {
            if(!CanAddRiverToCell(cell, edge, flow)) {
                throw new InvalidOperationException("CanAddRiverToCell must return true on the given arguments");
            }

            GetPresenceArray (cell)[(int)edge] = true;
            GetDirectionArray(cell)[(int)edge] = flow;

            var neighborAtEdge = Grid.GetNeighbor(cell, edge);

            if(neighborAtEdge != null) {
                GetPresenceArray (neighborAtEdge)[(int)(edge.Opposite())] = true;
                GetDirectionArray(neighborAtEdge)[(int)(edge.Opposite())] = flow.Opposite();
            }

            cell.RefreshSelfOnly();

            foreach(var neighbor in Grid.GetNeighbors(cell)) {
                neighbor.RefreshSelfOnly();
            }
        }

        public void OverrideRiverOnCell(IHexCell cell, HexDirection edge, RiverFlow flow) {
            GetPresenceArray (cell)[(int)edge] = true;
            GetDirectionArray(cell)[(int)edge] = flow;

            var neighborAtEdge = Grid.GetNeighbor(cell, edge);

            if(neighborAtEdge != null) {
                GetPresenceArray (neighborAtEdge)[(int)(edge.Opposite())] = true;
                GetDirectionArray(neighborAtEdge)[(int)(edge.Opposite())] = flow.Opposite();
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

        public void Clear() {
            RiverPresenceDict .Clear();
            RiverDirectionDict.Clear();
        }

        #endregion

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

        private bool RiverMeetsPlacementConditions(IHexCell cell, HexDirection edge, RiverFlow flow) {
            var right = Grid.GetNeighbor(cell, edge);     

            if(right == null) {
                return false;
            }

            if(cell.Terrain.IsWater() || right.Terrain.IsWater()) {
                return false;
            }

            IHexCell left      = Grid.GetNeighbor(cell, edge.Previous());
            IHexCell nextRight = Grid.GetNeighbor(cell, edge.Next());

            return PreviousCornerIsValid(cell, left,  right,     edge, flow)
                && NextCornerIsValid    (cell, right, nextRight, edge, flow);
        }

        private bool PreviousCornerIsValid(
            IHexCell cell, IHexCell left, IHexCell right, HexDirection edge, RiverFlow flow
        ) {
            if(left == null) {
                return true;
            }

            RiverFlow? centerLeftFlow = null;
            RiverFlow? leftRightFlow  = null;

            if(HasRiverAlongEdge(cell, edge.Previous())) {
                centerLeftFlow = GetFlowOfRiverAtEdge(cell, edge.Previous());
            }

            if(HasRiverAlongEdge(left, edge.Next())) {
                leftRightFlow = GetFlowOfRiverAtEdge(left, edge.Next());
            }

            return RiverCornerValidityLogic.AreCornerFlowsValid(flow, centerLeftFlow, leftRightFlow);
        }

        private bool NextCornerIsValid(
            IHexCell cell, IHexCell right, IHexCell nextRight, HexDirection edge, RiverFlow flow
        ) {
            if(nextRight == null) {
                return true;
            }          

            RiverFlow? centerNextRightFlow = null;
            RiverFlow? nextRightRightFlow  = null;

            if(HasRiverAlongEdge(cell, edge.Next())) {
                centerNextRightFlow = GetFlowOfRiverAtEdge(cell, edge.Next());
            }

            if(HasRiverAlongEdge(nextRight, edge.Previous())) {
                nextRightRightFlow = GetFlowOfRiverAtEdge(nextRight, edge.Previous());
            }

            return RiverCornerValidityLogic.AreCornerFlowsValid(flow, centerNextRightFlow, nextRightRightFlow);
        }

        #endregion

    }

}
