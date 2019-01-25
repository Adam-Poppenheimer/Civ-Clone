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

        HexMesh SmoothTerrain    { get; }
        HexMesh JaggedTerrain    { get; }
        HexMesh Roads            { get; }
        HexMesh Rivers           { get; }
        HexMesh RiverConfluences { get; }
        HexMesh RiverCorners     { get; }
        HexMesh Water            { get; }
        HexMesh Culture          { get; }
        HexMesh WaterShore       { get; }
        HexMesh Estuaries        { get; }
        HexMesh Marsh            { get; }
        HexMesh FloodPlains      { get; }
        HexMesh Oases            { get; }

        #endregion

        #region methods

        void ClearMeshes();
        void ApplyMeshes();

        CellTriangulationData GetTriangulationData(
            IHexCell center, IHexCell left, IHexCell right, HexDirection direction
        );

        void TriangulateEdgeFan(
            Vector3 center, EdgeVertices edge, float index,
            HexMesh targetMesh, bool perturbY = false
        );

        void TriangulateEdgeFan(
            Vector3 center, EdgeVertices edge, float index,
            float v, HexMesh targetedMesh, bool perturbY = false
        );

        void TriangulateEdgeStrip(
            EdgeVertices e1, Color w1, float index1,
            EdgeVertices e2, Color w2, float index2,
            HexMesh targetMesh
        );

        void TriangulateEdgeStrip(
            EdgeVertices e1, Color w1, float index1, bool perturbEdgeOneY,
            EdgeVertices e2, Color w2, float index2, bool perturbEdgeTwoY,
            HexMesh targetMesh
        );

        void TriangulateEdgeStrip(
            EdgeVertices e1, Color w1, float index1, float v1, bool perturbEdgeOneY,
            EdgeVertices e2, Color w2, float index2, float v2, bool perturbEdgeTwoY,
            HexMesh targetMesh
        );

        void TriangulateEdgeStrip(
            EdgeVertices e1, Color w1, float index1, float v1, bool perturbEdgeOneY,
            EdgeVertices e2, Color w2, float index2, float v2, bool perturbEdgeTwoY,
            Color color, HexMesh targetMesh
        );

        void TriangulateEdgeStripPartial(
            EdgeVertices e1, Color w1, float index1, float v1, bool perturbEdgeOneY,
            EdgeVertices e2, Color w2, float index2, float v2, bool perturbEdgeTwoY,
            HexMesh targetMesh, bool includeV1, bool includeV5
        );

        void TriangulateEdgeStripPartial(
            EdgeVertices e1, Color w1, float index1, float v1, bool perturbEdgeOneY,
            EdgeVertices e2, Color w2, float index2, float v2, bool perturbEdgeTwoY,
            Color color, HexMesh targetMesh, bool includeV1, bool includeV5
        );

        void TriangulateEdgeStripUnperturbed(
            EdgeVertices edgeOne, Color weightsOne, float indexOne,
            EdgeVertices edgeTwo, Color weightsTwo, float indexTwo,
            HexMesh targetMesh
        );

        void TriangulateEdgeStripUnperturbed(
            EdgeVertices edgeOne, Color weightsOnes,
            EdgeVertices edgeTwo, Color weightsTwo,
            Vector3 indices, HexMesh targetMesh
        );

        void TriangulateEdgeStripUnperturbed(
            EdgeVertices e1, Color w1, float index1, float v1,
            EdgeVertices e2, Color w2, float index2, float v2,
            HexMesh targetMesh
        );

        void TriangulateEdgeStripUnperturbed(
            EdgeVertices e1, Color w1, float index1, float v1,
            EdgeVertices e2, Color w2, float index2, float v2,
            Color color, HexMesh targetMesh
        );

        void AddTriangle(
            Vector3 vertexOne,   int indexOne,   Color weightsOne,
            Vector3 vertexTwo,   int indexTwo,   Color weightsTwo,
            Vector3 vertexThree, int indexThree, Color weightsThree,
            HexMesh targetMesh
        );

        void AddTriangle(
            Vector3 vertexOne,   Color weightsOne,   Vector2 uv1,
            Vector3 vertexTwo,   Color weightsTwo,   Vector2 uv2,
            Vector3 vertexThree, Color weightsThree, Vector2 uv3,
            Vector3 indices, HexMesh targetMesh
        );

        void AddTriangleUnperturbed(
            Vector3 vertexOne,   int indexOne,   Color weightsOne,
            Vector3 vertexTwo,   int indexTwo,   Color weightsTwo,
            Vector3 vertexThree, int indexThree, Color weightsThree,
            HexMesh targetMesh
        );

        void AddTriangleUnperturbed(
            Vector3 vertexOne,   Color weightsOne,
            Vector3 vertexTwo,   Color weightsTwo,
            Vector3 vertexThree, Color weightsThree,
            Vector3 indices, HexMesh targetMesh
        );

        void AddTriangleUnperturbed(
            Vector3 vertexOne,   Color weightsOne,   Vector2 uv1,
            Vector3 vertexTwo,   Color weightsTwo,   Vector2 uv2,
            Vector3 vertexThree, Color weightsThree, Vector2 uv3,
            Vector3 indices, HexMesh targetMesh
        );

        void AddTriangleUnperturbed(
            Vector3 vertexOne,   Color weightsOne,   Vector2 uv1,
            Vector3 vertexTwo,   Color weightsTwo,   Vector2 uv2,
            Vector3 vertexThree, Color weightsThree, Vector2 uv3,
            Color color, Vector3 indices, HexMesh targetMesh
        );

        void AddQuad(
            Vector3 bottomLeft, Color weightsOne,   Vector3 bottomRight, Color weightsTwo,
            Vector3 topLeft,    Color weightsThree, Vector3 topRight,    Color weightsFour,
            int indexOne, int indexTwo, int indexThree, HexMesh targetMesh
        );

        void AddQuad(
            Vector3 bottomLeft,  Color weightsBL, Vector2 uvBL,
            Vector3 bottomRight, Color weightsBR, Vector2 uvBR,
            Vector3 topLeft,     Color weightsTL, Vector2 uvTL,
            Vector3 topRight,    Color weightsTR, Vector2 uvTR,
            Color color, Vector3 indices, HexMesh targetMesh
        );

        void AddQuad(
            Vector3 bottomLeft,  Color weightsBL, Vector2 uvBL,
            Vector3 bottomRight, Color weightsBR, Vector2 uvBR,
            Vector3 topLeft,     Color weightsTL, Vector2 uvTL,
            Vector3 topRight,    Color weightsTR, Vector2 uvTR,
            Vector3 indices, HexMesh targetMesh
        );

        void AddQuadUnperturbed(
            Vector3 bottomLeft, Color weightsOne,   Vector3 bottomRight, Color weightsTwo,
            Vector3 topLeft,    Color weightsThree, Vector3 topRight,    Color weightsFour,
            int indexOne, int indexTwo, int indexThree, HexMesh targetMesh
        );

        void AddQuadUnperturbed(
            Vector3 bottomLeft, Color weightsOne,   Vector3 bottomRight, Color weightsTwo,
            Vector3 topLeft,    Color weightsThree, Vector3 topRight,    Color weightsFour,
            Vector3 indices, HexMesh targetMesh
        );

        void AddQuadUnperturbed(
            Vector3 bottomLeft,  Color weightsBL, Vector2 uvBL,
            Vector3 bottomRight, Color weightsBR, Vector2 uvBR,
            Vector3 topLeft,     Color weightsTL, Vector2 uvTL,
            Vector3 topRight,    Color weightsTR, Vector2 uvTR,
            Color color, Vector3 indices, HexMesh targetMesh
        );

        void AddQuadUnperturbed(
            Vector3 bottomLeft,  Color weightsBL, Vector2 uvBL,
            Vector3 bottomRight, Color weightsBR, Vector2 uvBR,
            Vector3 topLeft,     Color weightsTL, Vector2 uvTL,
            Vector3 topRight,    Color weightsTR, Vector2 uvTR,
            Vector3 indices, HexMesh targetMesh
        );

        void AddQuadUnperturbed(
            Vector3 bottomLeft,  Color weightsBL, Vector2 uvBL, Vector2 uv2BL,
            Vector3 bottomRight, Color weightsBR, Vector2 uvBR, Vector2 uv2BR,
            Vector3 topLeft,     Color weightsTL, Vector2 uvTL, Vector2 uv2TL,
            Vector3 topRight,    Color weightsTR, Vector2 uvTR, Vector2 uv2TR,
            Vector3 indices, HexMesh targetMesh
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

        #endregion

    }

}