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

        RiverFlow GetFlowOfRiverAtEdge(IHexCell cell, HexDirection edge);

        bool CanAddRiverToCell(IHexCell cell, HexDirection edge, RiverFlow direction);
        void AddRiverToCell   (IHexCell cell, HexDirection edge, RiverFlow direction);

        void RemoveRiverFromCellInDirection(IHexCell cell, HexDirection edge);

        void RemoveAllRiversFromCell(IHexCell cell);

        void ValidateRivers(IHexCell cell);

        void Clear();

        #endregion

    }

}