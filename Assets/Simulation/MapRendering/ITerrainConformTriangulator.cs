using UnityEngine;

namespace Assets.Simulation.MapRendering {

    public interface ITerrainConformTriangulator {

        #region methods

        void AddConformingQuad(
            Vector3 bottomLeftVertex,  Vector2 bottomLeftUV,  Color bottomLeftColor,
            Vector3 bottomRightVertex, Vector2 bottomRightUV, Color bottomRightColor,
            Vector3 topLeftVertex,     Vector2 topLeftUV,     Color topLeftColor,
            Vector3 topRightVertex,    Vector2 topRightUV,    Color topRightColor,
            IHexMesh mesh
        );

        void AddConformingTriangle(
            Vector3 vertex1, Vector2 uv1, Color color1,
            Vector3 vertex2, Vector2 uv2, Color color2,
            Vector3 vertex3, Vector2 uv3, Color color3,
            IHexMesh mesh
        );

        #endregion

    }

}