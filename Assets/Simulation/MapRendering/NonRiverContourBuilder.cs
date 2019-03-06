using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapRendering {

    public class NonRiverContourBuilder : INonRiverContourBuilder {

        #region instance fields and properties

        private IHexGrid              Grid;
        private IRiverCanon           RiverCanon;
        private ICellEdgeContourCanon CellEdgeContourCanon;
        private IMapRenderConfig      RenderConfig;

        #endregion

        #region constructors

        [Inject]
        public NonRiverContourBuilder(
            IHexGrid grid, IRiverCanon riverCanon, ICellEdgeContourCanon cellEdgeContourCanon,
            IMapRenderConfig renderConfig
        ) {
            Grid                 = grid;
            RiverCanon           = riverCanon;
            CellEdgeContourCanon = cellEdgeContourCanon;
            RenderConfig         = renderConfig;
        }

        #endregion

        #region instance methods

        #region from INonRiverContourBuilder

        public void BuildNonRiverContours() {
            foreach(var center in Grid.Cells) {
                foreach(var direction in EnumUtil.GetValues<HexDirection>()) {
                    if(RiverCanon.HasRiverAlongEdge(center, direction)) {
                        continue;
                    }

                    var contourPoints = new List<Vector2>();

                    IHexCell left      = Grid.GetNeighbor(center, direction.Previous());
                    IHexCell right     = Grid.GetNeighbor(center, direction);
                    IHexCell nextRight = Grid.GetNeighbor(center, direction.Next());

                    bool hasCenterLeftRiver      = RiverCanon.HasRiverAlongEdge(center, direction.Previous());
                    bool hasCenterNextRightRiver = RiverCanon.HasRiverAlongEdge(center, direction.Next());

                    bool hasLeftRightRiver      = left      != null && RiverCanon.HasRiverAlongEdge(left,      direction.Next());
                    bool hasNextRightRightRiver = nextRight != null && RiverCanon.HasRiverAlongEdge(nextRight, direction.Previous());

                    if(hasCenterLeftRiver) {
                        ICollection<Vector2> centerLeftContour = CellEdgeContourCanon.GetContourForCellEdge(center, direction.Previous());

                        contourPoints.Add(
                            RiverCanon.GetFlowOfRiverAtEdge(center, direction.Previous()) == RiverFlow.Clockwise
                                ? direction.Previous() <= HexDirection.SE ? centerLeftContour.Last () : centerLeftContour.First()
                                : direction.Previous() <= HexDirection.SE ? centerLeftContour.First() : centerLeftContour.Last ()
                        );

                    } else if(hasLeftRightRiver) {
                        ICollection<Vector2> rightLeftContour = CellEdgeContourCanon.GetContourForCellEdge(right, direction.Previous2());

                        contourPoints.Add(
                            RiverCanon.GetFlowOfRiverAtEdge(right, direction.Previous2()) == RiverFlow.Clockwise
                                ? direction.Previous2() <= HexDirection.SE ? rightLeftContour.First() : rightLeftContour.Last ()
                                : direction.Previous2() <= HexDirection.SE ? rightLeftContour.Last () : rightLeftContour.First()
                        );

                    } else {
                        contourPoints.Add(center.AbsolutePositionXZ + RenderConfig.GetFirstCornerXZ(direction));
                    }

                    if(hasCenterNextRightRiver) {
                        ICollection<Vector2> centerNextRightContour = CellEdgeContourCanon.GetContourForCellEdge(center, direction.Next());

                        contourPoints.Add(
                            RiverCanon.GetFlowOfRiverAtEdge(center, direction.Next()) == RiverFlow.Clockwise
                                ? direction.Next() <= HexDirection.SE ? centerNextRightContour.First() : centerNextRightContour.Last ()
                                : direction.Next() <= HexDirection.SE ? centerNextRightContour.Last () : centerNextRightContour.First()
                        );
                    }else if(hasNextRightRightRiver) {
                        ICollection<Vector2> rightNextRightContour = CellEdgeContourCanon.GetContourForCellEdge(right,     direction.Next2());
                        ICollection<Vector2> nextRightRightContour = CellEdgeContourCanon.GetContourForCellEdge(nextRight, direction.Previous());

                        contourPoints.Add(
                            RiverCanon.GetFlowOfRiverAtEdge(right, direction.Next2()) == RiverFlow.Clockwise
                                ? direction.Next2() <= HexDirection.SE ? rightNextRightContour.Last () : rightNextRightContour.First()
                                : direction.Next2() <= HexDirection.SE ? rightNextRightContour.First() : rightNextRightContour.Last ()
                        );

                        contourPoints.Add(
                            RiverCanon.GetFlowOfRiverAtEdge(nextRight, direction.Previous()) == RiverFlow.Clockwise
                                ? direction.Previous() <= HexDirection.SE ? nextRightRightContour.First() : nextRightRightContour.Last ()
                                : direction.Previous() <= HexDirection.SE ? nextRightRightContour.Last () : nextRightRightContour.First()
                        );

                    } else {
                        contourPoints.Add(center.AbsolutePositionXZ + RenderConfig.GetSecondCornerXZ(direction));
                    }

                    CellEdgeContourCanon.SetContourForCellEdge(center, direction, contourPoints);
                }
            }
        }

        #endregion

        #endregion

    }

}
