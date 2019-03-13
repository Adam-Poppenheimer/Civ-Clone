using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapRendering {

    public class RiverSplineTester : MonoBehaviour {

        #region instance fields and properties

        private IRiverSplineBuilder   RiverSplineBuilder;
        private IRiverAssemblyCanon   RiverAssemblyCanon;
        private ICellEdgeContourCanon CellEdgeContourCanon;
        private IHexGrid              Grid;
        private IMapRenderConfig      RenderConfig;

        #endregion

        #region instance methods

        [Inject]
        private void InjectDependencies(
            IRiverSplineBuilder riverSplineBuilder, IRiverAssemblyCanon riverAssemblyCanon,
            ICellEdgeContourCanon cellEdgeContourCanon, IHexGrid grid,
            IMapRenderConfig renderConfig
        ) {
            RiverSplineBuilder   = riverSplineBuilder;
            RiverAssemblyCanon   = riverAssemblyCanon;
            CellEdgeContourCanon = cellEdgeContourCanon;
            Grid                 = grid;
            RenderConfig         = renderConfig;
        }

        #region Unity messages

        private void OnDrawGizmos() {
            if(RiverSplineBuilder == null) {
                return;
            }

            OnDrawGizmos_Contours();
        }

        private void OnDrawGizmos_Contours() {
            Vector3 lineOne = Vector3.zero, lineTwo = Vector3.zero;

            foreach(var cell in Grid.Cells) {
                Vector3 center = cell.AbsolutePosition;

                foreach(var direction in EnumUtil.GetValues<HexDirection>()) {
                    var contour = CellEdgeContourCanon.GetContourForCellEdge(cell, direction);

                    if(contour.Count == 0) {
                        continue;
                    }

                    for(int i = 0; i < contour.Count - 1; i++) {
                        Gizmos.color = Color.white;

                        lineOne.x = contour[i].x;
                        lineOne.z = contour[i].y;

                        lineTwo.x = contour[i + 1].x;
                        lineTwo.z = contour[i + 1].y;

                        Gizmos.DrawLine(lineOne, lineTwo);
                        
                        Gizmos.color = Color.gray;

                        lineTwo.x = contour[i].x;
                        lineTwo.z = contour[i].y;

                        Gizmos.DrawLine(center, lineTwo);
                    }

                    lineTwo.x = contour.Last().x;
                    lineTwo.z = contour.Last().y;

                    Gizmos.DrawLine(center, lineTwo);

                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(center, center + RenderConfig.GetFirstCorner (direction));
                    Gizmos.DrawLine(center, center + RenderConfig.GetSecondCorner(direction));
                }
            }
        }

        #endregion

        #endregion

    }

}
