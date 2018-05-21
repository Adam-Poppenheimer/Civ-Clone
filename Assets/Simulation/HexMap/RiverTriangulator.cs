using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Util;

namespace Assets.Simulation.HexMap {

    public class RiverTriangulator : IRiverTriangulator {

        #region instance fields and properties

        private IHexGrid            Grid;
        private IHexGridMeshBuilder MeshBuilder;
        private IRiverCanon         RiverCanon;
        private INoiseGenerator     NoiseGenerator;

        #endregion

        #region constructors

        [Inject]
        public RiverTriangulator(
            IHexGrid grid, IHexGridMeshBuilder meshBuilder, IRiverCanon riverCanon,
            INoiseGenerator noiseGenerator
        ) {
            Grid           = grid;
            MeshBuilder    = meshBuilder;
            RiverCanon     = riverCanon;
            NoiseGenerator = noiseGenerator;
        }

        #endregion

        #region instance methods

        #region from IRiverTriangulator

        public void TriangulateConnectionAsRiver(
            HexDirection direction, IHexCell cell, EdgeVertices nearEdge
        ){
            IHexCell neighbor = Grid.GetNeighbor(cell, direction);

            if(neighbor == null) {
                return;
            }

            var neighborCenter = neighbor.transform.localPosition;

            var farEdge = new EdgeVertices(
                neighborCenter + HexMetrics.GetSecondOuterSolidCorner(direction.Opposite()),
                neighborCenter + HexMetrics.GetFirstOuterSolidCorner (direction.Opposite())
            );

            farEdge.V1.y = neighbor.EdgeY;
            farEdge.V2.y = neighbor.EdgeY;
            farEdge.V3.y = neighbor.EdgeY;
            farEdge.V4.y = neighbor.EdgeY;
            farEdge.V5.y = neighbor.EdgeY;

            var middleEdge = new EdgeVertices(
                (nearEdge.V1 + farEdge.V1) / 2f,
                (nearEdge.V2 + farEdge.V2) / 2f,
                (nearEdge.V3 + farEdge.V3) / 2f,
                (nearEdge.V4 + farEdge.V4) / 2f,
                (nearEdge.V5 + farEdge.V5) / 2f
            );

            middleEdge.V1.y = Mathf.Min(nearEdge.V1.y, farEdge.V1.y) + HexMetrics.StreamBedElevationOffset;
            middleEdge.V2.y = Mathf.Min(nearEdge.V2.y, farEdge.V2.y) + HexMetrics.StreamBedElevationOffset;
            middleEdge.V3.y = Mathf.Min(nearEdge.V3.y, farEdge.V3.y) + HexMetrics.StreamBedElevationOffset;
            middleEdge.V4.y = Mathf.Min(nearEdge.V4.y, farEdge.V4.y) + HexMetrics.StreamBedElevationOffset;
            middleEdge.V5.y = Mathf.Min(nearEdge.V5.y, farEdge.V5.y) + HexMetrics.StreamBedElevationOffset;

            Color middleWeights = Color.Lerp(MeshBuilder.Weights1, MeshBuilder.Weights2, 0.5f);

            MeshBuilder.TriangulateEdgeStrip(
                nearEdge,   MeshBuilder.Weights1, cell.Index,     cell.Shape == TerrainShape.Hills,
                middleEdge, middleWeights,        neighbor.Index, false,
                false
            );

            var previousNeighbor = Grid.GetNeighbor(cell, direction.Previous());

            if(previousNeighbor != null) {
                var nearCorner     = HexMetrics.GetFirstCorner (direction)                       + cell            .transform.localPosition;
                var farCorner      = HexMetrics.GetSecondCorner(direction.Opposite())            + neighbor        .transform.localPosition;
                var previousCorner = HexMetrics.GetFirstCorner (direction.Previous().Opposite()) + previousNeighbor.transform.localPosition;

                var previousConfluence = (nearCorner + farCorner + previousCorner) / 3f;

                previousConfluence.y = Mathf.Min(nearCorner.y, farCorner.y, previousCorner.y) + HexMetrics.StreamBedElevationOffset;

                MeshBuilder.AddTerrainTriangleUnperturbed(
                    NoiseGenerator.Perturb(nearEdge.V1,        cell.Shape == TerrainShape.Hills), cell            .Index, MeshBuilder.Weights1,
                    NoiseGenerator.Perturb(previousConfluence, false),                            previousNeighbor.Index, MeshBuilder.Weights123,
                    NoiseGenerator.Perturb(middleEdge.V1,      false),                            neighbor        .Index, MeshBuilder.Weights12
                );

                if(!RiverCanon.HasRiverAlongEdge(neighbor, direction.Opposite().Next())) {
                    TriangulateRiverCornerWall(cell, previousNeighbor, neighbor, direction, farEdge, previousConfluence);
                }
            }            

            var nextNeighbor = Grid.GetNeighbor(cell, direction.Next());

            if(nextNeighbor != null) {
                var nearCorner = HexMetrics.GetSecondCorner(direction)                   + cell        .transform.localPosition;
                var farCorner  = HexMetrics.GetFirstCorner (direction.Opposite())        + neighbor    .transform.localPosition;
                var nextCorner = HexMetrics.GetSecondCorner(direction.Next().Opposite()) + nextNeighbor.transform.localPosition;

                var nextConfluence = (nearCorner + farCorner + nextCorner) / 3f;

                nextConfluence.y = Mathf.Min(nearCorner.y, farCorner.y, nextCorner.y) + HexMetrics.StreamBedElevationOffset;

                MeshBuilder.AddTerrainTriangleUnperturbed(
                    NoiseGenerator.Perturb(nearEdge.V5,    cell.Shape == TerrainShape.Hills), cell        .Index, MeshBuilder.Weights1,
                    NoiseGenerator.Perturb(middleEdge.V5,  false),                            neighbor    .Index, MeshBuilder.Weights12,
                    NoiseGenerator.Perturb(nextConfluence, false),                            nextNeighbor.Index, MeshBuilder.Weights123                 
                );
            }
        }

        private void TriangulateRiverCornerWall(
            IHexCell cell, IHexCell previousNeighbor, IHexCell neighbor,
            HexDirection direction, EdgeVertices farEdge, Vector3 previousConfluence
        ){
            var previousPoint = previousNeighbor.transform.localPosition
                    + HexMetrics.GetFirstOuterSolidCorner(direction.Previous().Opposite());

                previousPoint.y = previousNeighbor.EdgeY;

                var farPoint = farEdge.V1;

            var wallEdge     = HexMetrics.GetEdgeType(previousNeighbor, neighbor);
            var neighborEdge = HexMetrics.GetEdgeType(neighbor, cell);

            if(wallEdge == HexEdgeType.Flat) {
                MeshBuilder.AddTerrainTriangleUnperturbed(
                    NoiseGenerator.Perturb(previousPoint,      previousNeighbor.Shape == TerrainShape.Hills), previousNeighbor.Index, MeshBuilder.Weights1,
                    NoiseGenerator.Perturb(farPoint,           neighbor.Shape         == TerrainShape.Hills), neighbor        .Index, MeshBuilder.Weights2,
                    NoiseGenerator.Perturb(previousConfluence, false),                                        cell            .Index, MeshBuilder.Weights123
                );
            }else if(wallEdge == HexEdgeType.Slope) {
                if(neighborEdge == HexEdgeType.Slope) {

                }else {
                    if(previousNeighbor.EdgeElevation < neighbor.EdgeElevation) {
                        TriangulateRiverCornerWall_TerracesClockwiseUp(
                            cell, previousNeighbor, neighbor,
                            direction, farEdge, previousConfluence,
                            previousPoint, farPoint
                        );
                    }else {
                        TriangulateRiverCornerWall_TerracesClockwiseDown(
                            cell, previousNeighbor, neighbor,
                            direction, farEdge, previousConfluence,
                            previousPoint, farPoint
                        );
                    }
                }
            }else if(wallEdge == HexEdgeType.Cliff) {

            }            
        }

        private void TriangulateRiverCornerWall_TerracesClockwiseUp(
            IHexCell cell, IHexCell previousNeighbor, IHexCell neighbor,
            HexDirection direction, EdgeVertices farEdge, Vector3 previousConfluence,
            Vector3 previousPoint, Vector3 farPoint
        ) {
            var perturbedConfluence = NoiseGenerator.Perturb(previousConfluence);
            var perturbedFar        = NoiseGenerator.Perturb(farPoint, true);
            var perturbedPrevious   = NoiseGenerator.Perturb(previousPoint);

            var convergencePoint = (
                perturbedConfluence + perturbedFar
            ) / 2f;

            var lerpParam = Vector3Extensions.InverseLerp(perturbedConfluence, perturbedFar, convergencePoint);

            var convergenceWeights = Color.Lerp(MeshBuilder.Weights123, MeshBuilder.Weights2, lerpParam);

            MeshBuilder.AddTerrainTriangleUnperturbed(
                NoiseGenerator.Perturb(previousPoint, false),      previousNeighbor.Index, MeshBuilder.Weights1,
                convergencePoint,                                  neighbor.Index,         convergenceWeights,
                NoiseGenerator.Perturb(previousConfluence, false), cell.Index,             MeshBuilder.Weights123
            );

            Vector3 v2 = NoiseGenerator.Perturb(HexMetrics.TerraceLerp(previousPoint, farPoint, 1));
            Color w2 = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights2, 1);

            MeshBuilder.AddTerrainTriangleUnperturbed(
                perturbedPrevious, previousNeighbor.Index, MeshBuilder.Weights1,
                v2,                neighbor.Index,         w2,
                convergencePoint,  cell.Index,             convergenceWeights
            );

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 v1 = v2;
                Color w1 = w2;

                v2 = NoiseGenerator.Perturb(HexMetrics.TerraceLerp(previousPoint, farPoint, i));
                w2 = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights2, i);

                MeshBuilder.AddTerrainTriangleUnperturbed(
                    v1,                previousNeighbor.Index, w1,
                    v2,                neighbor.Index,         w2,
                    convergencePoint,  cell.Index,             convergenceWeights
                );
            }

            MeshBuilder.AddTerrainTriangleUnperturbed(
                v2,               previousNeighbor.Index, w2,
                perturbedFar,     neighbor.Index,         MeshBuilder.Weights2,
                convergencePoint, cell.Index,             convergenceWeights
            );
        }

        private void TriangulateRiverCornerWall_TerracesClockwiseDown(
            IHexCell cell, IHexCell previousNeighbor, IHexCell neighbor,
            HexDirection direction, EdgeVertices farEdge, Vector3 previousConfluence,
            Vector3 previousPoint, Vector3 farPoint
        ) {
            var perturbedConfluence = NoiseGenerator.Perturb(previousConfluence);
            var perturbedFar        = NoiseGenerator.Perturb(farPoint);
            var perturbedPrevious   = NoiseGenerator.Perturb(previousPoint, true);

            var convergencePoint = (
                perturbedConfluence + perturbedPrevious
            ) / 2f;

            var lerpParam = Vector3Extensions.InverseLerp(perturbedConfluence, perturbedPrevious, convergencePoint);

            var convergenceWeights = Color.Lerp(MeshBuilder.Weights123, MeshBuilder.Weights1, lerpParam);

            MeshBuilder.AddTerrainTriangleUnperturbed(
                convergencePoint,    previousNeighbor.Index, convergenceWeights,
                perturbedFar,        neighbor.Index,         MeshBuilder.Weights2,                
                perturbedConfluence, cell.Index,             MeshBuilder.Weights123                
            );

            Vector3 v2 = NoiseGenerator.Perturb(HexMetrics.TerraceLerp(farPoint, previousPoint, 1));
            Color w2 = HexMetrics.TerraceLerp(MeshBuilder.Weights3, MeshBuilder.Weights1, 1);

            MeshBuilder.AddTerrainTriangleUnperturbed(
                convergencePoint,  previousNeighbor.Index, convergenceWeights,
                v2,                cell.Index,             w2,
                perturbedFar,      neighbor.Index,         MeshBuilder.Weights3
            );

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 v1 = v2;
                Color w1 = w2;

                v2 = NoiseGenerator.Perturb(HexMetrics.TerraceLerp(farPoint, previousPoint, i));
                w2 = HexMetrics.TerraceLerp(MeshBuilder.Weights3, MeshBuilder.Weights1, i);

                MeshBuilder.AddTerrainTriangleUnperturbed(
                    convergencePoint,  previousNeighbor.Index, convergenceWeights,
                    v2,                cell.Index,             w2,
                    v1,                neighbor.Index,         w1
                );
            }

            MeshBuilder.AddTerrainTriangleUnperturbed(
                convergencePoint,  previousNeighbor.Index, convergenceWeights,
                perturbedPrevious, cell.Index,             MeshBuilder.Weights1,
                v2,                neighbor.Index,         w2
                
            );
        }

        public void TriangulateEstuary(
            IHexCell cell, IHexCell underwaterNeighbor, IHexCell nextNeighbor,
            HexDirection direction
        ){
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(cell.transform.position, 2);

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(underwaterNeighbor.transform.position, 2);

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(nextNeighbor.transform.position, 2);
        }

        #endregion

        #endregion

    }

}
