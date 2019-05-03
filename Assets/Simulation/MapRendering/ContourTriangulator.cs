using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Util;

namespace Assets.Simulation.MapRendering {

    public class ContourTriangulator : IContourTriangulator {

        #region instance fields and properites

        private ICellEdgeContourCanon CellContourCanon;
        private IRiverCanon           RiverCanon;
        private IHexGrid              Grid;

        #endregion

        #region constructors

        [Inject]
        public ContourTriangulator(
            ICellEdgeContourCanon cellContourCanon, IRiverCanon riverCanon, IHexGrid grid
        ) {
            CellContourCanon = cellContourCanon;
            RiverCanon       = riverCanon;
            Grid             = grid;
        }

        #endregion

        #region instance methods

        #region from IContourTriangulator

        public void TriangulateContoursBetween(
            IHexCell center, IHexCell right, HexDirection direction, Color centerWeights, Color rightWeights, IHexMesh mesh
        ) {
            var centerRightContour = CellContourCanon.GetContourForCellEdge(center, direction);
            var rightCenterContour = CellContourCanon.GetContourForCellEdge(right,  direction.Opposite());

            int centerRightIndex = 1, rightCenterIndex = rightCenterContour.Count - 1;

            while(centerRightIndex < centerRightContour.Count && rightCenterIndex > 0) {
                mesh.AddQuad(
                    centerRightContour[centerRightIndex - 1].ToXYZ(), centerRightContour[centerRightIndex   ].ToXYZ(),
                    rightCenterContour[rightCenterIndex    ].ToXYZ(), rightCenterContour[rightCenterIndex- 1].ToXYZ()
                );

                mesh.AddQuadColor(centerWeights, rightWeights);

                centerRightIndex++;
                rightCenterIndex--;
            }

            for(; centerRightIndex < centerRightContour.Count; centerRightIndex++) {
                mesh.AddTriangle(
                    centerRightContour[centerRightIndex - 1].ToXYZ(), rightCenterContour[0].ToXYZ(), centerRightContour[centerRightIndex].ToXYZ()
                );

                mesh.AddTriangleColor(centerWeights, rightWeights, centerWeights);
            }

            for(; rightCenterIndex > 0; rightCenterIndex--) {
                mesh.AddTriangle(
                    centerRightContour.Last().ToXYZ(), rightCenterContour[rightCenterIndex].ToXYZ(), rightCenterContour[rightCenterIndex - 1].ToXYZ()
                );

                mesh.AddTriangleColor(centerWeights, rightWeights, rightWeights);
            }

            if(RiverCanon.HasRiverAlongEdge(right, direction.Next2())) {
                var nextRight = Grid.GetNeighbor(center, direction.Next());

                var rightNextRightContour = CellContourCanon.GetContourForCellEdge(right,     direction.Next2   ());                
                var nextRightRightContour = CellContourCanon.GetContourForCellEdge(nextRight, direction.Previous());

                mesh.AddTriangle(
                    centerRightContour   .Last ().ToXYZ(),
                    rightNextRightContour.Last ().ToXYZ(),
                    nextRightRightContour.First().ToXYZ()
                );

                mesh.AddTriangleColor(centerWeights, rightWeights, rightWeights);
            }
        }

        #endregion

        #endregion

    }

}
