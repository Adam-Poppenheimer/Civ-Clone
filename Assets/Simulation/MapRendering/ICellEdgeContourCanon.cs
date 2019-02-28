using System.Collections.Generic;
using Assets.Simulation.HexMap;
using UnityEngine;

namespace Assets.Simulation.MapRendering {

    public interface ICellEdgeContourCanon {

        #region methods

        void SetContourForCellEdge(IHexCell cell, HexDirection edge, List<Vector3> contour);

        List<Vector3> GetContourForCellEdge(IHexCell cell, HexDirection edge);

        void Clear();

        #endregion

    }

}