using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Util;

namespace Assets.Simulation.HexMap {

    public class FloodPlainsTriangulator : IFloodPlainsTriangulator {

        #region instance fields and properties

        private IHexGridMeshBuilder MeshBuilder;
        private INoiseGenerator     NoiseGenerator;

        #endregion

        #region constructors

        [Inject]
        public FloodPlainsTriangulator(
            IHexGridMeshBuilder meshBuilder, INoiseGenerator noiseGenerator
        ) {
            MeshBuilder    = meshBuilder;
            NoiseGenerator = noiseGenerator;
        }

        #endregion

        #region instance methods

        #region from IFloodPlainsTriangulator

        public bool ShouldTriangulateFloodPlainEdge(CellTriangulationData data) {
            return data.Center.Terrain == CellTerrain.FloodPlains
                && data.CenterToRightEdgeType == HexEdgeType.River;
        }

        public bool ShouldTriangulateFloodPlainCorner(CellTriangulationData data) {
            return HasInnerCorner(data) || HasOuterCorner(data) || HasLeftEndpoint(data) || HasRightEndpoint(data);
        }

        public void TriangulateFloodPlainEdge(CellTriangulationData data) {
            MeshBuilder.TriangulateEdgeStrip(
                data.CenterToRightInnerEdge, MeshBuilder.Weights1, data.Center.Index, 0f,   false,
                data.CenterToRightEdge,      MeshBuilder.Weights1, data.Center.Index, 1.3f, false,
                MeshBuilder.FloodPlains
            );

            MeshBuilder.TriangulateEdgeStripUnperturbed(
                data.CenterToRightEdgePerturbed, MeshBuilder.Weights1, data.Center.Index, 1.5f,
                data.CenterRightTrough,          MeshBuilder.Weights2, data.Right .Index, 0f,
                MeshBuilder.FloodPlains
            );
        }

        public void TriangulateFloodPlainCorner(CellTriangulationData data) {
            if(HasInnerCorner(data)) {
                TriangulateInnerCorner(data);
            }

            if(HasOuterCorner(data)) {
                TriangulateOuterCorner(data);
            }

            if(HasLeftEndpoint(data)) {
                TriangulateLeftEndpoint(data);
            }

            if(HasRightEndpoint(data)) {
                TriangulateRightEndpoint(data);
            }
        }

        #endregion

        private bool HasInnerCorner(CellTriangulationData data) {
            return data.Center.Terrain == CellTerrain.FloodPlains
                && data.CenterToRightEdgeType == HexEdgeType.River
                && data.CenterToLeftEdgeType  == HexEdgeType.River;
        }

        private bool HasOuterCorner(CellTriangulationData data) {
            if(data.Left == null || data.Right == null) {
                return false;
            }else if(data.Left.Terrain != CellTerrain.FloodPlains && data.Right.Terrain != CellTerrain.FloodPlains) {
                return false;
            }else {
                return data.CenterToLeftEdgeType  == HexEdgeType.River
                    && data.CenterToRightEdgeType == HexEdgeType.River
                    && data.LeftToRightEdgeType   != HexEdgeType.River;
            }
        }

        private bool HasRightEndpoint(CellTriangulationData data) {
            return data.CenterToLeftEdgeType  == HexEdgeType.River
                && data.CenterToRightEdgeType != HexEdgeType.River
                && data.LeftToRightEdgeType   != HexEdgeType.River
                && (data.Center.Terrain == CellTerrain.FloodPlains || data.Left.Terrain == CellTerrain.FloodPlains);
        }

        private bool HasLeftEndpoint(CellTriangulationData data) {
            return data.CenterToRightEdgeType == HexEdgeType.River
                && data.CenterToLeftEdgeType  != HexEdgeType.River
                && data.LeftToRightEdgeType   != HexEdgeType.River
                && (data.Center.Terrain == CellTerrain.FloodPlains || data.Right.Terrain == CellTerrain.FloodPlains);
        }

        private void TriangulateInnerCorner(CellTriangulationData data) {
            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedCenterCorner,  MeshBuilder.Weights1,  new Vector2(0f, 1.5f),
                data.CenterLeftTroughPoint,  MeshBuilder.Weights12, new Vector2(0f, 0f),
                data.CenterRightTroughPoint, MeshBuilder.Weights13, new Vector2(0f, 0f),
                new Vector3(data.Center.Index, data.Left.Index, data.Right.Index),
                MeshBuilder.FloodPlains
            );
        }

        //The outer edge is between Left and Right. There are rivers between
        //Center/Left and Center/Right, but no river between Left and Right.
        private void TriangulateOuterCorner(CellTriangulationData data) {
            var indices = new Vector3(data.Center.Index, data.Left.Index, data.Right.Index);

            float leftY  = data.Left.LocalPosition.y;
            float rightY = data.Right.LocalPosition.y;

            Vector3 yAdjustedLeftCorner  = data.PerturbedLeftCorner;
            Vector3 yAdjustedRightCorner = data.PerturbedRightCorner;

            yAdjustedLeftCorner .y = leftY;
            yAdjustedRightCorner.y = rightY;

            float leftV   = data.Left  .Terrain == CellTerrain.FloodPlains ? 1.3f : 0f;
            float rightV  = data.Right .Terrain == CellTerrain.FloodPlains ? 1.3f : 0f;

            //If we draw a single quad between the corners and the nearby points of the inner edges,
            //the outer edges of our flood plains will be substantially thinner than the rest of them.
            //For this reason we need to have a more complex triangulation that tries to preserve
            //flood plain width. To do this, we create two intermediate points off of the left and
            //right corner whose distances from the river corner are the same as the distances
            //to each of the inner points. We then create a surface with these two points, the
            //two inner points, and the corner points.

            Vector3 perturbedLeftInner  = NoiseGenerator.Perturb(data.LeftToRightInnerEdge.V5);
            Vector3 perturbedRightInner = NoiseGenerator.Perturb(data.RightToLeftInnerEdge.V1);

            perturbedLeftInner .y = leftY;
            perturbedRightInner.y = rightY;

            float widthOfLeftPlains  = Vector3.Distance(perturbedLeftInner,  yAdjustedLeftCorner);
            float widthOfRightPlains = Vector3.Distance(perturbedRightInner, yAdjustedRightCorner);

            Vector3 leftTroughToLeftCorner   = yAdjustedLeftCorner  - data.CenterLeftTroughPoint;
            Vector3 rightTroughToRightCorner = yAdjustedRightCorner - data.CenterRightTroughPoint;

            Vector3 leftXZDirection  = new Vector3(leftTroughToLeftCorner  .x, 0f, leftTroughToLeftCorner  .z).normalized;
            Vector3 RightXZDirection = new Vector3(rightTroughToRightCorner.x, 0f, rightTroughToRightCorner.z).normalized;

            Vector3 leftIntermediatePoint  = yAdjustedLeftCorner  + leftXZDirection  * widthOfLeftPlains;
            Vector3 rightIntermediatePoint = yAdjustedRightCorner + RightXZDirection * widthOfRightPlains;

            MeshBuilder.AddTriangleUnperturbed(
                yAdjustedLeftCorner,   MeshBuilder.Weights2, new Vector2(0f, leftV),
                perturbedLeftInner,    MeshBuilder.Weights2, new Vector2(0f, 0f),
                leftIntermediatePoint, MeshBuilder.Weights2, new Vector2(0f, 0f),
                indices, MeshBuilder.FloodPlains
            );

            MeshBuilder.AddTriangleUnperturbed(
                yAdjustedRightCorner,   MeshBuilder.Weights3, new Vector2(0f, rightV),
                rightIntermediatePoint, MeshBuilder.Weights3, new Vector2(0f, 0f),
                perturbedRightInner,    MeshBuilder.Weights3, new Vector2(0f, 0f),
                indices, MeshBuilder.FloodPlains
            );

            MeshBuilder.AddQuadUnperturbed(
                yAdjustedLeftCorner,    MeshBuilder.Weights2, new Vector2(0f, leftV),
                yAdjustedRightCorner,   MeshBuilder.Weights3, new Vector2(0f, rightV),
                leftIntermediatePoint,  MeshBuilder.Weights2, new Vector2(0f, 0f),
                rightIntermediatePoint, MeshBuilder.Weights3, new Vector2(0f, 0f),                
                indices, MeshBuilder.FloodPlains
            );

            //This quad exists within the corner itself, connecting to the above surface
            //and extending down to both trough points
            MeshBuilder.AddQuadUnperturbed(                
                yAdjustedLeftCorner,         MeshBuilder.Weights2,  new Vector2(0f, leftV),
                data.CenterLeftTroughPoint,  MeshBuilder.Weights12, new Vector2(0f, 0f),  
                yAdjustedRightCorner,        MeshBuilder.Weights3,  new Vector2(0f, rightV),
                data.CenterRightTroughPoint, MeshBuilder.Weights13, new Vector2(0f, 0f),
                indices, MeshBuilder.FloodPlains
            );
        }

        //There's a river between Center and Left, but no rivers anywhere else.
        //Center is a flood plain.
        private void TriangulateRightEndpoint(CellTriangulationData data) {
            var indices = new Vector3(data.Center.Index, data.Left.Index, data.Right.Index);

            if(data.Center.Terrain == CellTerrain.FloodPlains) {
                //This triangle helps make sure the flood plain exends across
                //the entire length of our river segment.
                MeshBuilder.AddTriangleUnperturbed(
                    NoiseGenerator.Perturb(data.CenterToRightInnerEdge.V1), MeshBuilder.Weights1, new Vector2(0f, 0f),
                    data.PerturbedCenterCorner,                             MeshBuilder.Weights1, new Vector2(0f, 1.3f),
                    data.CenterToRightEdgePerturbed.V2,                     MeshBuilder.Weights1, new Vector2(0f, 0f),                
                    indices, MeshBuilder.FloodPlains
                );
            }

            //If there's a flat edge between center and right, we want to
            //continue our flood plain along the triangular river section
            //that's currently within the endpoint. It's already lined up
            //with CenterToRightEdge and RightToCenterEdge, so we can use
            //that to build a quad
            if(data.CenterToRightEdgeType == HexEdgeType.Flat) {
                float centerV = data.Center.Terrain == CellTerrain.FloodPlains ? 1.3f : 0f;
                float rightV = data.Right.Terrain.IsDesert() ? 1.3f : 0f;

                MeshBuilder.AddQuadUnperturbed(
                    data.CenterToRightEdgePerturbed.V1, MeshBuilder.Weights1, new Vector2(0f, centerV),
                    data.CenterToRightEdgePerturbed.V2, MeshBuilder.Weights1, new Vector2(0f, 0f),
                    data.RightToCenterEdgePerturbed.V1, MeshBuilder.Weights3, new Vector2(0f, rightV),
                    data.RightToCenterEdgePerturbed.V2, MeshBuilder.Weights3, new Vector2(0f, 0f),
                    indices, MeshBuilder.FloodPlains
                );

                //The above quad doesn't get us all the way around the endpoint.
                //We need to add an additional triangle (reflected in TriangulateLeftEndpoint)
                //To make sure things wrap properly.
                Vector3 endpointCap = (data.RightToCenterEdgePerturbed.V2 + data.RightToLeftEdgePerturbed.V2) / 2f;

                MeshBuilder.AddTriangleUnperturbed(
                    data.RightToCenterEdgePerturbed.V2, MeshBuilder.Weights3, new Vector2(0f, 0f),
                    data.RightToCenterEdgePerturbed.V1, MeshBuilder.Weights3, new Vector2(0f, rightV),
                    endpointCap,                        MeshBuilder.Weights3, new Vector2(0f, 0f),
                    indices, MeshBuilder.FloodPlains
                );
            }
        }

        //This method is a rotation of TriangulateRightEndpoint, swapping left
        //for right
        private void TriangulateLeftEndpoint(CellTriangulationData data) {
            var indices = new Vector3(data.Center.Index, data.Left.Index, data.Right.Index);

            if(data.Center.Terrain == CellTerrain.FloodPlains) {
                MeshBuilder.AddTriangleUnperturbed(
                    NoiseGenerator.Perturb(data.CenterToRightInnerEdge.V1), MeshBuilder.Weights1, new Vector2(0f, 0f),
                    data.CenterToLeftEdgePerturbed.V4,                      MeshBuilder.Weights1, new Vector2(0f, 0f),
                    data.PerturbedCenterCorner,                             MeshBuilder.Weights1, new Vector2(0f, 1.3f),
                    indices, MeshBuilder.FloodPlains
                );
            }

            if(data.CenterToLeftEdgeType == HexEdgeType.Flat) {
                float centerV = data.Center.Terrain == CellTerrain.FloodPlains ? 1.3f : 0f;
                float leftV = data.Left.Terrain.IsDesert() ? 1.3f : 0f;

                MeshBuilder.AddQuadUnperturbed(
                    data.CenterToLeftEdgePerturbed.V4, MeshBuilder.Weights1, new Vector2(0f, 0f),
                    data.CenterToLeftEdgePerturbed.V5, MeshBuilder.Weights1, new Vector2(0f, centerV),
                    data.LeftToCenterEdgePerturbed.V4, MeshBuilder.Weights2, new Vector2(0f, 0f),
                    data.LeftToCenterEdgePerturbed.V5, MeshBuilder.Weights2, new Vector2(0f, leftV),
                    indices, MeshBuilder.FloodPlains
                );

                Vector3 endpointCap = (data.LeftToCenterEdgePerturbed.V4 + data.LeftToRightEdgePerturbed.V4) / 2f;

                MeshBuilder.AddTriangleUnperturbed(
                    data.LeftToCenterEdgePerturbed.V5, MeshBuilder.Weights2, new Vector2(0f, leftV),
                    data.LeftToCenterEdgePerturbed.V4, MeshBuilder.Weights2, new Vector2(0f, 0f),
                    endpointCap,                       MeshBuilder.Weights2, new Vector2(0f, 0f),
                    indices, MeshBuilder.FloodPlains
                );
            }
        }

        #endregion

    }

}
