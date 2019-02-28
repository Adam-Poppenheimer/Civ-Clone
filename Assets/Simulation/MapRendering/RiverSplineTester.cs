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

        [SerializeField] private bool DrawStartPoints;
        [SerializeField] private bool DrawEndPoints;
        [SerializeField] private bool DrawIntermediatePoints;



        private IRiverSplineBuilder   RiverSplineBuilder;
        private IRiverAssemblyCanon   RiverAssemblyCanon;
        private ICellEdgeContourCanon CellEdgeContourCanon;
        private IHexGrid              Grid;
        private IRiverCanon           RiverCanon;

        #endregion

        #region instance methods

        [Inject]
        private void InjectDependencies(
            IRiverSplineBuilder riverSplineBuilder, IRiverAssemblyCanon riverAssemblyCanon,
            ICellEdgeContourCanon cellEdgeContourCanon, IHexGrid grid, IRiverCanon riverCanon
        ) {
            RiverSplineBuilder   = riverSplineBuilder;
            RiverAssemblyCanon   = riverAssemblyCanon;
            CellEdgeContourCanon = cellEdgeContourCanon;
            Grid                 = grid;
            RiverCanon           = riverCanon;
        }

        #region Unity messages

        private void OnDrawGizmos() {
            if(RiverSplineBuilder == null) {
                return;
            }

            OnDrawGizmos_Contours();
        }

        private void OnDrawGizmos_River() {
            foreach(var riverSpline in RiverSplineBuilder.LastBuiltRiverSplines) {
                if(DrawStartPoints) {
                    Gizmos.color = Color.yellow;

                    Gizmos.DrawSphere(riverSpline.CenterSpline.Points[0], 1f);
                }

                if(DrawIntermediatePoints) {
                    Gizmos.color = Color.blue;

                    for(int i = 1; i < riverSpline.CenterSpline.Points.Count - 1; i++) {
                        Gizmos.DrawSphere(riverSpline.CenterSpline.Points[i], 1f);
                    }
                }

                if(DrawEndPoints && riverSpline.CenterSpline.Points.Count > 1) {
                    Gizmos.color = Color.green;

                    Gizmos.DrawSphere(riverSpline.CenterSpline.Points.Last(), 1f);

                    Gizmos.color = Color.blue;
                }

                float tDelta = 1f / (100f * riverSpline.CenterSpline.CurveCount);

                for(float t = 0f; t < 1f; t += tDelta) {
                    Gizmos.DrawLine(riverSpline.CenterSpline.GetPoint(t), riverSpline.CenterSpline.GetPoint(t + tDelta));
                }
            }

            Gizmos.color = Color.red;

            foreach(RiverSection section in RiverAssemblyCanon.UnassignedSections) {
                Gizmos.DrawLine(section.Start, section.End);
            }
        }

        private void OnDrawGizmos_Contours() {
            foreach(var cell in Grid.Cells) {
                Vector3 center = cell.AbsolutePosition;

                foreach(var direction in EnumUtil.GetValues<HexDirection>()) {
                    var contour = CellEdgeContourCanon.GetContourForCellEdge(cell, direction);

                    for(int i = 0; i < contour.Count - 1; i++) {
                        Gizmos.color = Color.white;
                        Gizmos.DrawLine(contour[i], contour[i + 1]);

                        Gizmos.color = Color.gray;
                        Gizmos.DrawLine(center, contour[i]);
                    }

                    Gizmos.DrawLine(center, contour.Last());
                }
            }
        }

        #endregion

        #endregion

    }

}
