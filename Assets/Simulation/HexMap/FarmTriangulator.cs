using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Improvements;

namespace Assets.Simulation.HexMap {

    public class FarmTriangulator : IFarmTriangulator {

        #region internal types

        private class FarmCorner {

            public Vector3 Position;
            public Vector2 UV;

            public FarmCorner(Vector3 position, Vector2 uv) {
                Position = position;
                UV       = uv;
            }

        }

        private class FarmQuad {

            #region instance fields and properties

            public FarmCorner BottomLeft;
            public FarmCorner BottomRight;
            public FarmCorner TopLeft;
            public FarmCorner TopRight;

            public Vector3 Midpoint {
                get { return (BottomLeft.Position + BottomRight.Position + TopLeft.Position + TopRight.Position) / 4f; }
            }

            #endregion

            #region instance methods

            public FarmQuad RotateClockwise() {
                return new FarmQuad() {
                    BottomLeft  = this.BottomRight,
                    BottomRight = this.TopRight,
                    TopLeft     = this.BottomLeft,
                    TopRight    = this.TopLeft
                };
            }

            #endregion

        }

        private class FarmData {

            #region instance fields and properties

            public Vector3 MiddlePoint { get { return NoiseGenerator.Perturb(Cell.LocalPosition); } }

            public Vector3 NorthPoint     { get { return NoiseGenerator.Perturb(Cell.LocalPosition + HexMetrics.GetFirstOuterSolidCorner(HexDirection.NE)); } }
            public Vector3 NorthEastPoint { get { return NoiseGenerator.Perturb(Cell.LocalPosition + HexMetrics.GetFirstOuterSolidCorner(HexDirection.E));  } }
            public Vector3 SouthEastPoint { get { return NoiseGenerator.Perturb(Cell.LocalPosition + HexMetrics.GetFirstOuterSolidCorner(HexDirection.SE)); } }
            public Vector3 SouthPoint     { get { return NoiseGenerator.Perturb(Cell.LocalPosition + HexMetrics.GetFirstOuterSolidCorner(HexDirection.SW)); } }
            public Vector3 SouthWestPoint { get { return NoiseGenerator.Perturb(Cell.LocalPosition + HexMetrics.GetFirstOuterSolidCorner(HexDirection.W));  } }
            public Vector3 NorthWestPoint { get { return NoiseGenerator.Perturb(Cell.LocalPosition + HexMetrics.GetFirstOuterSolidCorner(HexDirection.NW)); } }

            public Vector3 NorthNorthEastPoint { get { return (NorthPoint + NorthEastPoint) / 2f; } }
            public Vector3 SouthSouthEastPoint { get { return (SouthPoint + SouthEastPoint) / 2f; } }
            public Vector3 SouthSouthWestPoint { get { return (SouthPoint + SouthWestPoint) / 2f; } }
            public Vector3 NorthNorthWestPoint { get { return (NorthPoint + NorthWestPoint) / 2f; } }

            public Vector3 MiddleNorthPoint { get { return (MiddlePoint    + NorthPoint)     / 2f; } }
            public Vector3 MiddleEastPoint  { get { return (NorthEastPoint + SouthEastPoint) / 2f; } }
            public Vector3 MiddleSouthPoint { get { return (MiddlePoint    + SouthPoint)     / 2f; } }
            public Vector3 MiddleWestPoint  { get { return (NorthWestPoint + SouthWestPoint) / 2f; } }
            
            public Vector3 MiddleNorthEastPoint { get { return (MiddlePoint + NorthEastPoint) / 2f; } }
            public Vector3 MiddleSouthEastPoint { get { return (MiddlePoint + SouthEastPoint) / 2f; } }
            public Vector3 MiddleSouthWestPoint { get { return (MiddlePoint + SouthWestPoint) / 2f; } }
            public Vector3 MiddleNorthWestPoint { get { return (MiddlePoint + NorthWestPoint) / 2f; } }


            private IHexCell Cell;

            private INoiseGenerator NoiseGenerator;

            #endregion

            #region constructors

            public FarmData(IHexCell cell, INoiseGenerator noiseGenerator) {
                Cell = cell;

                NoiseGenerator = noiseGenerator;
            }

            #endregion

        }

        #endregion

        #region instance fields and properties

        private IImprovementLocationCanon ImprovementLocationCanon;
        private IImprovementTemplate      FarmTemplate;
        private IHexGridMeshBuilder       MeshBuilder;
        private INoiseGenerator           NoiseGenerator;
        private IHexMapConfig             Config;

        #endregion

        #region constructors

        [Inject]
        public FarmTriangulator(
            IImprovementLocationCanon improvementLocationCanon,
            [Inject(Id = "Farm Template")] IImprovementTemplate farmTemplate,
            IHexGridMeshBuilder meshBuilder, INoiseGenerator noiseGenerator,
            IHexMapConfig config
        ) {
            ImprovementLocationCanon = improvementLocationCanon;
            FarmTemplate             = farmTemplate;
            MeshBuilder              = meshBuilder;
            NoiseGenerator           = noiseGenerator;
            Config                   = config;
        }

        #endregion

        #region instance methods

        #region from IFarmTriangulator

        public bool ShouldTriangulateFarmCenter(CellTriangulationData data) {
            return HasFarm(data.Center);
        }

        public void TriangulateFarmCenter(CellTriangulationData data) {
            if(data.Center.Shape == CellShape.Hills) {
                TriangulateCenterWithHills(data);
            }else {
                TriangulateCenterWithoutHills(data);
            }
        }

        public bool ShouldTriangulateFarmEdge(CellTriangulationData data) {
            return HasFarm(data.Center) && HasFarm(data.Right) && data.Center.Shape == data.Right.Shape;
        }

        public void TriangulateFarmEdge(CellTriangulationData data) {
            if(data.Center.Shape == CellShape.Hills) {
                MeshBuilder.TriangulateEdgeStrip(
                    data.CenterToRightEdge, MeshBuilder.Weights1, data.Center.Index, data.Center.RequiresYPerturb,
                    data.RightToCenterEdge, MeshBuilder.Weights2, data.Right .Index, data.Right .RequiresYPerturb,
                    MeshBuilder.Farmland
                );
            }else {
                MeshBuilder.TriangulateEdgeStrip(
                    data.CenterToRightEdge, MeshBuilder.Weights1, data.Center.Index,
                    data.RightToCenterEdge, MeshBuilder.Weights2, data.Right .Index,
                    MeshBuilder.Farmland
                );
            }
        }

        public bool ShouldTriangulateFarmCorner(CellTriangulationData data) {
            return HasFarm(data.Center) && HasFarm(data.Left) && HasFarm(data.Right)
                && data.Center.Shape == data.Left.Shape
                && data.Center.Shape == data.Right.Shape;
        }

        public void TriangulateFarmCorner(CellTriangulationData data) {
            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedCenterCorner, data.Center.Index, MeshBuilder.Weights1,
                data.PerturbedLeftCorner,   data.Left  .Index, MeshBuilder.Weights2,
                data.PerturbedRightCorner,  data.Right .Index, MeshBuilder.Weights3,
                MeshBuilder.Farmland
            );
        }

        #endregion

        private void TriangulateCenterWithHills(CellTriangulationData data) {
            MeshBuilder.TriangulateEdgeFan(
                data.CenterPeak, data.CenterToRightInnerEdge,
                data.Center.Index, MeshBuilder.Farmland, true
            );

            MeshBuilder.TriangulateEdgeStrip(
                data.CenterToRightInnerEdge, MeshBuilder.Weights1, data.Center.Index, true,
                data.CenterToRightEdge,      MeshBuilder.Weights1, data.Center.Index, true,
                MeshBuilder.Farmland
            );
        }

        private void TriangulateCenterWithoutHills(CellTriangulationData data){
            MeshBuilder.TriangulateEdgeFan(
                data.CenterPeak, data.CenterToRightEdge, data.Center.Index,
                MeshBuilder.Farmland
            );
        } 

        private bool HasFarm(IHexCell cell) {
            if(cell == null) {
                return false;
            }

            var improvementAt = ImprovementLocationCanon.GetPossessionsOfOwner(cell).FirstOrDefault();

            return improvementAt != null && improvementAt.Template == FarmTemplate;
        }

        #endregion

    }

}
