﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.HexMap {

    public class HexGridMeshBuilder : IHexGridMeshBuilder {

        #region static fields and properties

        private static Color _weights1 = new Color(1f, 0f, 0f);
        private static Color _weights2 = new Color(0f, 1f, 0f);
        private static Color _weights3 = new Color(0f, 0f, 1f);

        private static Color _weights12 = new Color(0.5f, 0.5f, 0f);
        private static Color _weights13 = new Color(0.5f, 0f,   0.5f);
        private static Color _weights23 = new Color(0f,   0.5f, 0.5f);

        private static Color _weights123 = new Color(0.3333f, 0.3333f, 0.3333f);

        #endregion

        #region instance fields and properties

        #region from HexGridMeshBuilder

        public Color Weights1 {
            get { return _weights1; }
        }

        public Color Weights2 {
            get { return _weights2; }
        }

        public Color Weights3 {
            get { return _weights3; }
        }

        public Color Weights12 {
            get { return _weights12; }
        }

        public Color Weights13 {
            get { return _weights13; }
        }

        public Color Weights23 {
            get { return _weights23; }
        }

        public Color Weights123 {
            get { return _weights123; }
        }

        #endregion

        public HexMesh Terrain    { get; private set; }
        public HexMesh Roads      { get; private set; }
        public HexMesh Rivers     { get; private set; }
        public HexMesh Water      { get; private set; }
        public HexMesh Culture    { get; private set; }
        public HexMesh WaterShore { get; private set; }
        public HexMesh Estuaries  { get; private set; }

        private INoiseGenerator NoiseGenerator;
        private IRiverCanon     RiverCanon;

        #endregion

        #region constructors

        public HexGridMeshBuilder(
            [Inject(Id = "Terrain")]     HexMesh terrain,
            [Inject(Id = "Roads")]       HexMesh roads,
            [Inject(Id = "Rivers")]      HexMesh rivers,
            [Inject(Id = "Water")]       HexMesh water,
            [Inject(Id = "Culture")]     HexMesh culture,
            [Inject(Id = "Water Shore")] HexMesh waterShore,
            [Inject(Id = "Estuaries")]   HexMesh estuaries,
            INoiseGenerator noiseGenerator, IRiverCanon riverCanon
        ) {
            Terrain    = terrain;
            Roads      = roads;
            Rivers     = rivers;
            Water      = water;
            Culture    = culture;
            WaterShore = waterShore;
            Estuaries  = estuaries;

            NoiseGenerator = noiseGenerator;
            RiverCanon     = riverCanon;
        }

        #endregion

        #region instance methods

        #region from HexGridMeshBuilder

        public void ClearMeshes() {
            Terrain   .Clear();
            Rivers    .Clear();
            Roads     .Clear();
            Water     .Clear();
            WaterShore.Clear();
            Estuaries .Clear();
            Culture   .Clear();
        }

        public void ApplyMeshes() {
            Terrain   .Apply();
            Rivers    .Apply();
            Roads     .Apply();
            Water     .Apply();
            WaterShore.Apply();
            Estuaries .Apply();
            Culture   .Apply();
        }

        public CellTriangulationData GetTriangulationData(
            IHexCell center, IHexCell left, IHexCell right, HexDirection direction
        ) {
            return new CellTriangulationData(center, left, right, direction, NoiseGenerator, RiverCanon);
        }

        public void TriangulateEdgeFan(
            Vector3 center, EdgeVertices edge, float index, HexMesh targetedMesh, bool perturbY = false
        ){
            targetedMesh.AddTriangle(center, edge.V1, edge.V2, perturbY);
            targetedMesh.AddTriangle(center, edge.V2, edge.V3, perturbY);
            targetedMesh.AddTriangle(center, edge.V3, edge.V4, perturbY);
            targetedMesh.AddTriangle(center, edge.V4, edge.V5, perturbY);

            Vector3 indices;
            indices.x = indices.y = indices.z = index;

            targetedMesh.AddTriangleCellData(indices, Weights1);
            targetedMesh.AddTriangleCellData(indices, Weights1);
            targetedMesh.AddTriangleCellData(indices, Weights1);
            targetedMesh.AddTriangleCellData(indices, Weights1);
        }

        public void TriangulateEdgeStrip(
            EdgeVertices e1, Color w1, float index1,
            EdgeVertices e2, Color w2, float index2,
            HexMesh targetedMesh
        ) {
            targetedMesh.AddQuad(e1.V1, e1.V2, e2.V1, e2.V2);
            targetedMesh.AddQuad(e1.V2, e1.V3, e2.V2, e2.V3);
            targetedMesh.AddQuad(e1.V3, e1.V4, e2.V3, e2.V4);
            targetedMesh.AddQuad(e1.V4, e1.V5, e2.V4, e2.V5);

            Vector3 indices;
            indices.x = indices.z = index1;
            indices.y = index2;

            targetedMesh.AddQuadCellData(indices, w1, w2);
            targetedMesh.AddQuadCellData(indices, w1, w2);
            targetedMesh.AddQuadCellData(indices, w1, w2);
            targetedMesh.AddQuadCellData(indices, w1, w2);
        }

        public void TriangulateEdgeStrip(
            EdgeVertices e1, Color w1, float index1, bool perturbEdgeOneY,
            EdgeVertices e2, Color w2, float index2, bool perturbEdgeTwoY,
            HexMesh targetMesh
        ) {
            targetMesh.AddQuadUnperturbed(
                NoiseGenerator.Perturb(e1.V1, perturbEdgeOneY), NoiseGenerator.Perturb(e1.V2, perturbEdgeOneY),
                NoiseGenerator.Perturb(e2.V1, perturbEdgeTwoY), NoiseGenerator.Perturb(e2.V2, perturbEdgeTwoY)
            );

            targetMesh.AddQuadUnperturbed(
                NoiseGenerator.Perturb(e1.V2, perturbEdgeOneY), NoiseGenerator.Perturb(e1.V3, perturbEdgeOneY),
                NoiseGenerator.Perturb(e2.V2, perturbEdgeTwoY), NoiseGenerator.Perturb(e2.V3, perturbEdgeTwoY)
            );

            targetMesh.AddQuadUnperturbed(
                NoiseGenerator.Perturb(e1.V3, perturbEdgeOneY), NoiseGenerator.Perturb(e1.V4, perturbEdgeOneY),
                NoiseGenerator.Perturb(e2.V3, perturbEdgeTwoY), NoiseGenerator.Perturb(e2.V4, perturbEdgeTwoY)
            );

            targetMesh.AddQuadUnperturbed(
                NoiseGenerator.Perturb(e1.V4, perturbEdgeOneY), NoiseGenerator.Perturb(e1.V5, perturbEdgeOneY),
                NoiseGenerator.Perturb(e2.V4, perturbEdgeTwoY), NoiseGenerator.Perturb(e2.V5, perturbEdgeTwoY)
            );

            Vector3 indices;
            indices.x = indices.z = index1;
            indices.y = index2;

            targetMesh.AddQuadCellData(indices, w1, w2);
            targetMesh.AddQuadCellData(indices, w1, w2);
            targetMesh.AddQuadCellData(indices, w1, w2);
            targetMesh.AddQuadCellData(indices, w1, w2);
        }

        public void TriangulateEdgeStrip(
            EdgeVertices e1, Color w1, float index1, float v1, bool perturbEdgeOneY,
            EdgeVertices e2, Color w2, float index2, float v2, bool perturbEdgeTwoY,
            HexMesh targetMesh
        ) {
            TriangulateEdgeStrip(
                e1, w1, index1, perturbEdgeOneY,
                e2, w2, index2, perturbEdgeTwoY,
                targetMesh
            );

            targetMesh.AddQuadUV(0f, 0f, v1, v2);
            targetMesh.AddQuadUV(0f, 0f, v1, v2);
            targetMesh.AddQuadUV(0f, 0f, v1, v2);
            targetMesh.AddQuadUV(0f, 0f, v1, v2);
        }

        public void TriangulateEdgeStrip(
            EdgeVertices e1, Color w1, float index1, float v1, bool perturbEdgeOneY,
            EdgeVertices e2, Color w2, float index2, float v2, bool perturbEdgeTwoY,
            Color color, HexMesh targetMesh
        ) {
            TriangulateEdgeStrip(
                e1, w1, index1, perturbEdgeOneY,
                e2, w2, index2, perturbEdgeTwoY,
                targetMesh
            );

            targetMesh.AddQuadColor(color);
            targetMesh.AddQuadColor(color);
            targetMesh.AddQuadColor(color);
            targetMesh.AddQuadColor(color);

            targetMesh.AddQuadUV(0f, 0f, v1, v2);
            targetMesh.AddQuadUV(0f, 0f, v1, v2);
            targetMesh.AddQuadUV(0f, 0f, v1, v2);
            targetMesh.AddQuadUV(0f, 0f, v1, v2);
        }

        public void TriangulateRoadSegment(
            Vector3 v1, Vector3 v2, Vector3 v3,
            Vector3 v4, Vector3 v5, Vector3 v6,
            Color w1, Color w2, Vector3 indices
        ) {
            Roads.AddQuad(v1, v2, v4, v5);
            Roads.AddQuad(v2, v3, v5, v6);

            Roads.AddQuadUV(0f, 1f, 0f, 0f);
            Roads.AddQuadUV(1f, 0f, 0f, 0f);

            Roads.AddQuadCellData(indices, w1, w2);
            Roads.AddQuadCellData(indices, w1, w2);
        }

        public void AddTriangle(
            Vector3 vertexOne,   int indexOne,   Color weightsOne,
            Vector3 vertexTwo,   int indexTwo,   Color weightsTwo,
            Vector3 vertexThree, int indexThree, Color weightsThree,
            HexMesh targetedMesh
        ) {
            targetedMesh.AddTriangle(vertexOne, vertexTwo, vertexThree);

            Vector3 indices = new Vector3(indexOne, indexTwo, indexThree);

            targetedMesh.AddTriangleCellData(indices, weightsOne, weightsTwo, weightsThree);
        }

        public void AddTriangle(
            Vector3 vertexOne,   Color weightsOne,   Vector2 uv1,
            Vector3 vertexTwo,   Color weightsTwo,   Vector2 uv2,
            Vector3 vertexThree, Color weightsThree, Vector2 uv3,
            Vector3 indices, HexMesh targetedMesh
        ) {
            targetedMesh.AddTriangle(vertexOne, vertexTwo, vertexThree);

            targetedMesh.AddTriangleUV(uv1, uv2, uv3);

            targetedMesh.AddTriangleCellData(indices, weightsOne, weightsTwo, weightsThree);
        }

        public void AddTriangleUnperturbed(
            Vector3 vertexOne,   int indexOne,   Color weightsOne,
            Vector3 vertexTwo,   int indexTwo,   Color weightsTwo,
            Vector3 vertexThree, int indexThree, Color weightsThree,
            HexMesh targetedMesh
        ) {
            targetedMesh.AddTriangleUnperturbed(vertexOne, vertexTwo, vertexThree);

            Vector3 indices = new Vector3(indexOne, indexTwo, indexThree);

            targetedMesh.AddTriangleCellData(indices, weightsOne, weightsTwo, weightsThree);
        }

        public void AddTriangleUnperturbed(
            Vector3 vertexOne,   Color weightsOne,
            Vector3 vertexTwo,   Color weightsTwo,
            Vector3 vertexThree, Color weightsThree,
            Vector3 indices, HexMesh targetedMesh
        ) {
            targetedMesh.AddTriangleUnperturbed(vertexOne, vertexTwo, vertexThree);

            targetedMesh.AddTriangleCellData(indices, weightsOne, weightsTwo, weightsThree);
        }

        public void AddTriangleUnperturbed(
            Vector3 vertexOne,   Color weightsOne,   Vector2 uv1,
            Vector3 vertexTwo,   Color weightsTwo,   Vector2 uv2,
            Vector3 vertexThree, Color weightsThree, Vector2 uv3,
            Vector3 indices, HexMesh targetedMesh
        ) {
            targetedMesh.AddTriangleUnperturbed(vertexOne, vertexTwo, vertexThree);

            targetedMesh.AddTriangleUV(uv1, uv2, uv3);

            targetedMesh.AddTriangleCellData(indices, weightsOne, weightsTwo, weightsThree);
        }

        public void AddTriangleUnperturbed(
            Vector3 vertexOne,   Color weightsOne,   Vector2 uv1,
            Vector3 vertexTwo,   Color weightsTwo,   Vector2 uv2,
            Vector3 vertexThree, Color weightsThree, Vector2 uv3,
            Color color, Vector3 indices, HexMesh targetedMesh
        ) {
            AddTriangleUnperturbed(
                vertexOne,   weightsOne,   uv1,
                vertexTwo,   weightsTwo,   uv2,
                vertexThree, weightsThree, uv3,
                indices, targetedMesh
            );

            targetedMesh.AddTriangleColor(color);
        }

        public void AddQuad(
            Vector3 bottomLeft, Color weightsOne,   Vector3 bottomRight, Color weightsTwo,
            Vector3 topLeft,    Color weightsThree, Vector3 topRight,    Color weightsFour,
            int indexOne, int indexTwo, int indexThree, HexMesh targetedMesh
        ) {
            targetedMesh.AddQuad(bottomLeft, bottomRight, topLeft, topRight);

            targetedMesh.AddQuadCellData(
                new Vector3(indexOne, indexTwo, indexThree),
                weightsOne, weightsTwo, weightsThree, weightsFour
            );
        }

        public void AddQuad(
            Vector3 bottomLeft,  Color weightsBL, Vector2 uvBL,
            Vector3 bottomRight, Color weightsBR, Vector2 uvBR,
            Vector3 topLeft,     Color weightsTL, Vector2 uvTL,
            Vector3 topRight,    Color weightsTR, Vector2 uvTR,
            Color color, Vector3 indices, HexMesh targetedMesh
        ) {
            targetedMesh.AddQuad(bottomLeft, bottomRight, topLeft, topRight);

            targetedMesh.AddQuadCellData(
                indices, weightsBL, weightsBR, weightsTL, weightsTR
            );

            targetedMesh.AddQuadUV(uvBL, uvBR, uvTL, uvTR);

            targetedMesh.AddQuadColor(color);
        }

        public void AddQuadUnperturbed(
            Vector3 bottomLeft, Color weightsOne,   Vector3 bottomRight, Color weightsTwo,
            Vector3 topLeft,    Color weightsThree, Vector3 topRight,    Color weightsFour,
            int indexOne, int indexTwo, int indexThree, HexMesh targetedMesh
        ) {
            targetedMesh.AddQuadUnperturbed(bottomLeft, bottomRight, topLeft, topRight);

            targetedMesh.AddQuadCellData(
                new Vector3(indexOne, indexTwo, indexThree),
                weightsOne, weightsTwo, weightsThree, weightsFour
            );
        }

        public void AddQuadUnperturbed(
            Vector3 bottomLeft,  Color weightsBL, Vector2 uvBL,
            Vector3 bottomRight, Color weightsBR, Vector2 uvBR,
            Vector3 topLeft,     Color weightsTL, Vector2 uvTL,
            Vector3 topRight,    Color weightsTR, Vector2 uvTR,
            Color color, Vector3 indices, HexMesh targetedMesh
        ) {
            targetedMesh.AddQuadUnperturbed(bottomLeft, bottomRight, topLeft, topRight);

            targetedMesh.AddQuadCellData(
                indices, weightsBL, weightsBR, weightsTL, weightsTR
            );

            targetedMesh.AddQuadUV(uvBL, uvBR, uvTL, uvTR);

            targetedMesh.AddQuadColor(color);
        }

        public void TriangulateRiverQuad(
            Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4,
            float y, float vMin, float vMax,
            bool isReversed, Vector3 indices
        ) {
            TriangulateRiverQuad(v1, v2, v3, v4, y, y, vMin, vMax, isReversed, indices);
        }

        public void TriangulateRiverQuad(
            Vector3 bottomLeft, Vector3 bottomRight,
            Vector3 topLeft, Vector3 topRight,
            float y1, float y2, float vMin, float vMax,
            bool isReversed, Vector3 indices
        ) {
            bottomLeft.y = bottomRight.y = y1;
            topLeft   .y = topRight   .y = y2;

            TriangulateRiverQuad(
                bottomLeft, bottomRight, topLeft, topRight,
                vMin, vMax, isReversed, indices
            );
        }

        public void TriangulateRiverQuad(
            Vector3 bottomLeft, Vector3 bottomRight,
            Vector3 topLeft, Vector3 topRight,
            float vMin, float vMax,
            bool isReversed, Vector3 indices
        ) {
            TriangulateRiverQuadUnperturbed(
                NoiseGenerator.Perturb(bottomLeft), NoiseGenerator.Perturb(bottomRight),
                NoiseGenerator.Perturb(topLeft),    NoiseGenerator.Perturb(topRight),
                vMin, vMax, isReversed, indices
            );
        }

        public void TriangulateRiverQuadUnperturbed(
            Vector3 bottomLeft, Vector3 bottomRight,
            Vector3 topLeft, Vector3 topRight,
            float vMin, float vMax,
            bool isReversed, Vector3 indices
        ) {
            Rivers.AddQuadUnperturbed(bottomLeft, bottomRight, topLeft, topRight);

            if(isReversed) {
                Rivers.AddQuadUV(0f, 1f, 1f - vMin, 1f - vMax);
            }else {
                Rivers.AddQuadUV(0f, 1f, vMin, vMax);
            }

            Rivers.AddQuadCellData(indices, Weights1, Weights2);
        }

        public void TriangulateRiverQuadUnperturbed(
            Vector3 bottomLeft, Vector3 bottomRight,
            Vector3 topLeft, Vector3 topRight,
            float y1, float y2, float vMin, float vMax,
            bool isReversed, Vector3 indices
        ) {
            bottomLeft.y = bottomRight.y = y1;
            topLeft   .y = topRight   .y = y2;

            TriangulateRiverQuadUnperturbed(
                bottomLeft, bottomRight, topLeft, topRight,
                vMin, vMax, isReversed, indices
            );
        }

        #endregion

        #endregion

    }

}
