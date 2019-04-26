using UnityEngine;

namespace Assets.Simulation.MapRendering {

    public interface IHexMesh {

        #region properties

        Transform transform { get; }

        int Layer { get; }

        bool ShouldBeBaked { get; }

        #endregion

        #region methods

        void AddQuad(Vector3 bottomLeft, Vector3 bottomRight, Vector3 topLeft, Vector3 topRight);

        void AddQuadCellData(Vector3 indices, Color weights);
        void AddQuadCellData(Vector3 indices, Color weights1, Color weights2);
        void AddQuadCellData(Vector3 indices, Color weights1, Color weights2, Color weights3, Color weights4);

        void AddQuadColor(Color color);
        void AddQuadColor(Color colorOne, Color colorTwo);
        void AddQuadColor(Color colorOne, Color colorTwo, Color colorThree, Color colorFour);

        void AddQuadUV(Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4);
        void AddQuadUV(float uMin, float uMax, float vMin, float vMax);

        void AddQuadUV2(Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4);
        void AddQuadUV2(float uMin, float uMax, float vMin, float vMax);

        void AddQuadUV3(Vector4 uv1, Vector4 uv2, Vector4 uv3, Vector4 uv4);

        void AddTriangle(Vector3 vertexOne, Vector3 vertexTwo, Vector3 vertexThree);

        void AddTriangleCellData(Vector3 indices, Color weights);
        void AddTriangleCellData(Vector3 indices, Color weights1, Color weights2, Color weights3);

        void AddTriangleColor(Color color);
        void AddTriangleColor(Color colorOne, Color colorTwo, Color colorThree);

        void AddTriangleUV(Vector2 uv1, Vector2 uv2, Vector2 uv3);
        void AddTriangleUV2(Vector2 uv1, Vector2 uv2, Vector2 uv3);

        void AddTriangleUV3(Vector4 uv1, Vector4 uv2, Vector4 uv3);

        void Apply();
        void Clear();

        void SetActive(bool isActive);

        void OverrideMaterial(Material newMaterial);

        #endregion

    }

}