using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.HexMap {

    public interface IRiverCanon {

        #region methods

        bool HasRiver(IHexCell cell);

        bool HasRiverBeginOrEnd(IHexCell cell);

        bool HasIncomingRiver(IHexCell cell);
        
        bool HasOutgoingRiver(IHexCell cell);

        HexDirection GetIncomingRiver(IHexCell cell);

        HexDirection GetOutgoingRiver(IHexCell cell);

        HexDirection GetRiverBeginOrEndDirection(IHexCell cell);

        bool HasRiverThroughEdge(IHexCell cell, HexDirection direction);

        bool CanAddOutgoingRiverThroughEdge(IHexCell cell, HexDirection direction);

        bool CanAddIncomingRiverThroughEdge(IHexCell cell, HexDirection direction);

        void SetOutgoingRiver(IHexCell cell, HexDirection direction);

        void RemoveOutgoingRiver(IHexCell cell);

        void RemoveRiver(IHexCell cell);

        void ValidateRivers(IHexCell cell);

        #endregion

    }

}