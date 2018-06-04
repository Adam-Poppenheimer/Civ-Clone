using UnityEngine;

namespace Assets.Simulation.HexMap {

    public interface IHexGridMeshBuilder {

        #region properties

        Color Weights1 { get; }
        Color Weights2 { get; }
        Color Weights3 { get; }

        Color Weights12 { get; }
        Color Weights13 { get; }
        Color Weights23 { get; }

        Color Weights123 { get; }

        #endregion

        #region methods

        void TriangulateEdgeFan(
            Vector3 center, EdgeVertices edge, float index, bool perturbY = false
        );

        void TriangulateEdgeStrip(
            EdgeVertices e1, Color w1, float index1,
            EdgeVertices e2, Color w2, float index2,
            bool hasRoad = false
        );

        void TriangulateEdgeStrip(
            EdgeVertices e1, Color w1, float index1, bool perturbEdgeOneY,
            EdgeVertices e2, Color w2, float index2, bool perturbEdgeTwoY,
            bool hasRoad = false
        );

        void TriangulateRoadSegment(
            Vector3 v1, Vector3 v2, Vector3 v3,
            Vector3 v4, Vector3 v5, Vector3 v6,
            Color w1, Color w2, Vector3 indices
        );

        void AddTerrainTriangle(
            Vector3 vertexOne,   int indexOne,   Color weightsOne,
            Vector3 vertexTwo,   int indexTwo,   Color weightsTwo,
            Vector3 vertexThree, int indexThree, Color weightsThree
        );

        void AddTerrainTriangleUnperturbed(
            Vector3 vertexOne,   int indexOne,   Color weightsOne,
            Vector3 vertexTwo,   int indexTwo,   Color weightsTwo,
            Vector3 vertexThree, int indexThree, Color weightsThree
        );

        void TriangulateRiverQuad(
            Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4,
            float y, float vMin, float vMax,
            bool isReversed, Vector3 indices
        );

        void TriangulateRiverQuad(
            Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4,
            float y1, float y2, float vMin, float vMax,
            bool isReversed, Vector3 indices
        );

        void TriangulateRiverQuad(
            Vector3 bottomLeft, Vector3 bottomRight,
            Vector3 topLeft, Vector3 topRight,
            float vMin, float vMax,
            bool isReversed, Vector3 indices
        );

        void TriangulateRiverQuadUnperturbed(
            Vector3 bottomLeft, Vector3 bottomRight,
            Vector3 topLeft, Vector3 topRight,
            float vMin, float vMax,
            bool isReversed, Vector3 indices
        );

        void TriangulateRiverQuadUnperturbed(
            Vector3 bottomLeft, Vector3 bottomRight,
            Vector3 topLeft, Vector3 topRight,
            float y1, float y2, float vMin, float vMax,
            bool isReversed, Vector3 indices
        );

        void AddRiverTriangleUnperturbed(
            Vector3 v1, Vector3 v2, Vector3 v3,
            Vector2 uv1, Vector2 uv2, Vector2 uv3,
            Vector3 indices
        );

        void AddWaterTriangleUnperturbed(
            Vector3 vertexOne,   int indexOne,   Color weightsOne,
            Vector3 vertexTwo,   int indexTwo,   Color weightsTwo,
            Vector3 vertexThree, int indexThree, Color weightsThree
        );

        #endregion

    }

}