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

        public void BuildNonRiverContour(IHexCell center, HexDirection direction) {
            if(RiverCanon.HasRiverAlongEdge(center, direction)) {
                return;
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

                contourPoints.Add(centerLeftContour.Last());

            } else if(hasLeftRightRiver) {
                ICollection<Vector2> rightLeftContour = CellEdgeContourCanon.GetContourForCellEdge(right, direction.Previous2());

                contourPoints.Add(rightLeftContour.First());

            } else {
                contourPoints.Add(center.AbsolutePositionXZ + RenderConfig.GetFirstCornerXZ(direction));
            }

            if( hasCenterNextRightRiver ||
                (hasNextRightRightRiver && RiverCanon.GetFlowOfRiverAtEdge(nextRight, direction.Previous()) == RiverFlow.Clockwise)
            ) {
                ICollection<Vector2> centerNextRightContour = CellEdgeContourCanon.GetContourForCellEdge(center, direction.Next());

                contourPoints.Add(centerNextRightContour.First());

            }else if(hasNextRightRightRiver) {
                //We need to add two points here in case the edge is up against the
                //outflow of a river. We've decided to give the outflow over entirely
                //to one of the cells, but other constructions are possible.
                ICollection<Vector2> rightNextRightContour = CellEdgeContourCanon.GetContourForCellEdge(right,     direction.Next2());
                ICollection<Vector2> nextRightRightContour = CellEdgeContourCanon.GetContourForCellEdge(nextRight, direction.Previous());

                contourPoints.Add(rightNextRightContour.Last());
                contourPoints.Add(nextRightRightContour.First());

            } else {
                contourPoints.Add(center.AbsolutePositionXZ + RenderConfig.GetSecondCornerXZ(direction));
            }

            CellEdgeContourCanon.SetContourForCellEdge(center, direction, contourPoints);
        }

        #endregion

        #endregion

    }

}
