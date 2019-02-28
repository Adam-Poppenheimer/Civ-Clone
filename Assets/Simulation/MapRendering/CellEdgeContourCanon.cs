using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public class CellEdgeContourCanon : ICellEdgeContourCanon {

        #region instance fields and properties

        private Dictionary<IHexCell, List<Vector3>[]> ContourOfEdgeOfCell =
            new Dictionary<IHexCell, List<Vector3>[]>();



        private IMapRenderConfig RenderConfig;

        #endregion

        #region constructors

        [Inject]
        public CellEdgeContourCanon(IMapRenderConfig renderConfig) {
            RenderConfig = renderConfig;
        }

        #endregion

        #region instance methods

        #region from ICellEdgeContourCanon

        public void SetContourForCellEdge(IHexCell cell, HexDirection edge, List<Vector3> contour) {
            List<Vector3>[] contourArray;

            if(!ContourOfEdgeOfCell.TryGetValue(cell, out contourArray)) {
                contourArray = new List<Vector3>[6];

                ContourOfEdgeOfCell[cell] = contourArray;
            }

            contourArray[(int)edge] = contour;
        }

        public List<Vector3> GetContourForCellEdge(IHexCell cell, HexDirection edge) {
            List<Vector3>[] contourArray;

            if(ContourOfEdgeOfCell.TryGetValue(cell, out contourArray) && contourArray[(int)edge] != null) {
                return contourArray[(int)edge];
            }else {
                return new List<Vector3>() {
                    cell.AbsolutePosition + RenderConfig.GetFirstCorner (edge),
                    cell.AbsolutePosition + RenderConfig.GetSecondCorner(edge)
                };
            }
        }

        public void Clear() {
            ContourOfEdgeOfCell.Clear();
        }

        #endregion

        #endregion

    }

}
