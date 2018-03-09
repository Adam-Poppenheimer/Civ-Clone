using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.HexMap {

    public class RiverCanon : IRiverCanon {

        #region instance fields and properties

        private Dictionary<IHexCell, bool[]> IncomingRivers = new Dictionary<IHexCell, bool[]>();
        private Dictionary<IHexCell, bool[]> OutgoingRivers = new Dictionary<IHexCell, bool[]>();

        private IHexGrid Grid;

        #endregion

        #region constructors

        public RiverCanon(IHexGrid grid) {
            Grid = grid;
        }

        #endregion

        #region instance methods

        #region from IRiverCanon        

        public HexDirection GetIncomingRiver(IHexCell cell) {
            var riverFlags = KeyIntoIncoming(cell);

            for(int i = 0; i < 6; i++) {
                if(riverFlags[i]) {
                    return (HexDirection)i;
                }
            }

            return HexDirection.NE;
        }

        public HexDirection GetOutgoingRiver(IHexCell cell) {
            var riverFlags = KeyIntoOutgoing(cell);

            for(int i = 0; i < 6; i++) {
                if(riverFlags[i]) {
                    return (HexDirection)i;
                }
            }

            return HexDirection.NE;
        }

        public HexDirection GetRiverBeginOrEndDirection(IHexCell cell) {
            return HasIncomingRiver(cell) ? GetIncomingRiver(cell) : GetOutgoingRiver(cell);
        }

        public bool HasIncomingRiver(IHexCell cell) {
            return KeyIntoIncoming(cell).Exists(flag => flag);
        }

        public bool HasOutgoingRiver(IHexCell cell) {
            return KeyIntoOutgoing(cell).Exists(flag => flag);
        }

        public bool HasRiver(IHexCell cell) {
            return HasIncomingRiver(cell) || HasOutgoingRiver(cell);
        }

        public bool HasRiverThroughEdge(IHexCell cell, HexDirection direction) {
            return KeyIntoIncoming(cell, direction) || KeyIntoOutgoing(cell, direction);
        }

        public bool HasRiverBeginOrEnd(IHexCell cell) {
            return HasIncomingRiver(cell) != HasOutgoingRiver(cell);
        }

        public bool CanAddOutgoingRiverThroughEdge(IHexCell cell, HexDirection direction) {
            var neighbor = Grid.GetNeighbor(cell, direction);

            if(neighbor != null) {
                bool validElevation = cell.FoundationElevation >= neighbor.FoundationElevation;
                bool validWaterLevel = cell.WaterLevel == neighbor.FoundationElevation;

                return validElevation || validWaterLevel;
            }
            return false;
        }

        public bool CanAddIncomingRiverThroughEdge(IHexCell cell, HexDirection direction) {
            var neighbor = Grid.GetNeighbor(cell, direction);

            if(neighbor != null) {
                bool validElevation = cell.FoundationElevation <= neighbor.FoundationElevation;
                bool validWaterLevel = cell.WaterLevel == neighbor.FoundationElevation && neighbor.IsUnderwater;

                return validElevation || validWaterLevel;
            }
            return false;
        }

        public void SetOutgoingRiver(IHexCell cell, HexDirection direction) {
            if(HasOutgoingRiver(cell) && GetOutgoingRiver(cell) == direction) {
                return;
            }

            if(!CanAddOutgoingRiverThroughEdge(cell, direction)) {
                return;
            }

            IHexCell neighbor = Grid.GetNeighbor(cell, direction);

            RemoveOutgoingRiver(cell);
            if(HasIncomingRiver(cell) && GetIncomingRiver(cell) == direction) {
                RemoveIncomingRiver(cell);
            }

            KeyIntoOutgoing(cell, direction, true);
            
            RemoveIncomingRiver(neighbor);
            KeyIntoIncoming(neighbor, direction.Opposite(), true);

            cell.RefreshSelfOnly();
            neighbor.RefreshSelfOnly();
        }

        public void RemoveOutgoingRiver(IHexCell cell) {
            if(!HasOutgoingRiver(cell)) {
                return;
            }

            HexDirection outgoingRiver = GetOutgoingRiver(cell);

            KeyIntoOutgoing(cell, outgoingRiver, false);
            cell.RefreshSelfOnly();

            IHexCell neighbor = Grid.GetNeighbor(cell, outgoingRiver);

            KeyIntoIncoming(neighbor, outgoingRiver.Opposite(), false);
            neighbor.RefreshSelfOnly();
        }

        public void RemoveRiver(IHexCell cell) {
            RemoveIncomingRiver(cell);
            RemoveOutgoingRiver(cell);
        }

        public void ValidateRivers(IHexCell cell) {
            if(HasOutgoingRiver(cell) && !CanAddOutgoingRiverThroughEdge(cell, GetOutgoingRiver(cell))) {
                RemoveOutgoingRiver(cell);
            }

            if(HasIncomingRiver(cell) && !CanAddIncomingRiverThroughEdge(cell, GetIncomingRiver(cell))) {
                RemoveIncomingRiver(cell);
            }
        }

        #endregion

        private void RemoveIncomingRiver(IHexCell cell) {
            if(!HasIncomingRiver(cell)) {
                return;
            }

            HexDirection incomingRiver = GetIncomingRiver(cell);

            KeyIntoIncoming(cell, incomingRiver, false);
            cell.RefreshSelfOnly();

            IHexCell neighbor = Grid.GetNeighbor(cell, incomingRiver);

            KeyIntoOutgoing(neighbor, incomingRiver.Opposite(), false);
            neighbor.RefreshSelfOnly();
        }

        private bool[] KeyIntoIncoming(IHexCell cell) {
            if(!IncomingRivers.ContainsKey(cell)) {
                IncomingRivers[cell] = new bool[6];
            }

            return IncomingRivers[cell];
        }

        private bool KeyIntoIncoming(IHexCell cell, HexDirection direction) {
            return KeyIntoIncoming(cell)[(int)direction];
        }

        private void KeyIntoIncoming(IHexCell cell, HexDirection direction, bool newValue) {
            KeyIntoIncoming(cell)[(int)direction] = newValue;
        }

        private bool[] KeyIntoOutgoing(IHexCell cell) {
            if(!OutgoingRivers.ContainsKey(cell)) {
                OutgoingRivers[cell] = new bool[6];
            }

            return OutgoingRivers[cell];
        }

        private bool KeyIntoOutgoing(IHexCell cell, HexDirection direction) {
            return KeyIntoOutgoing(cell)[(int)direction];
        }

        private void KeyIntoOutgoing(IHexCell cell, HexDirection direction, bool newValue) {
            KeyIntoOutgoing(cell)[(int)direction] = newValue;
        }

        #endregion

    }

}
