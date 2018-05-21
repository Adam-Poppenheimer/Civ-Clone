using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.HexMap {

    public interface IRiverCanon {

        #region methods

        bool HasRiver(IHexCell cell);

        bool HasRiverAlongEdge(IHexCell cell, HexDirection edge);

        IEnumerable<HexDirection> GetEdgesWithRivers(IHexCell cell);

        RiverDirection GetFlowDirectionOfRiverAtEdge(IHexCell cell, HexDirection edge);

        bool CanAddRiverToCell(IHexCell cell, HexDirection edge, RiverDirection direction);
        void AddRiverToCell   (IHexCell cell, HexDirection edge, RiverDirection direction);

        void RemoveRiverFromCellInDirection(IHexCell cell, HexDirection edge);

        void RemoveAllRiversFromCell(IHexCell cell);

        void ValidateRivers(IHexCell cell);

        #endregion

    }

}