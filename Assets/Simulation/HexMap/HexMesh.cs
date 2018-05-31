using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;

namespace Assets.Simulation.HexMap {

    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class HexMesh : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private bool UseCollider;
        [SerializeField] private bool UseUVCoordinates;
        [SerializeField] private bool UseUV2Coordinates;
        [SerializeField] private bool UseCellData;
        [SerializeField] private bool UseColors;

        private Mesh ManagedMesh;
        public MeshCollider Collider { get; private set; }

        [NonSerialized] private List<Vector3> Vertices;
        [NonSerialized] private List<int>     Triangles;
        [NonSerialized] private List<Color>   CellWeights;
        [NonSerialized] private List<Vector2> UVs;
        [NonSerialized] private List<Vector2> UV2s;
        [NonSerialized] private List<Vector3> CellIndices;
        [NonSerialized] private List<Color>   Colors;

        private INoiseGenerator NoiseGenerator;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(INoiseGenerator noiseGenerator) {
            NoiseGenerator = noiseGenerator;
        }

        #region Unity message methods

        private void Awake() {
            GetComponent<MeshFilter>().mesh = ManagedMesh = new Mesh();
            ManagedMesh.name = "Hex Mesh";
            
            if(UseCollider) {
                Collider = gameObject.AddComponent<MeshCollider>();
            }
        }        

        #endregion        

        public void Clear() {
            ManagedMesh.Clear();
            Vertices  = ListPool<Vector3>.Get();
            Triangles = ListPool<int>    .Get();

            if(UseCellData) {
                CellWeights = ListPool<Color>.Get();
                CellIndices = ListPool<Vector3>.Get();
            }

            if(UseUVCoordinates) {
                UVs = ListPool<Vector2>.Get();
            }

            if(UseUV2Coordinates) {
                UV2s = ListPool<Vector2>.Get();
            }

            if(UseColors) {
                Colors = ListPool<Color>.Get();
            }
        }

        public void Apply() {
            ManagedMesh.SetVertices(Vertices);
            ListPool<Vector3>.Add(Vertices);

            ManagedMesh.SetTriangles(Triangles, 0);
            ListPool<int>.Add(Triangles);

            if(UseCellData) {
                ManagedMesh.SetColors(CellWeights);
                ListPool<Color>.Add(CellWeights);

                ManagedMesh.SetUVs(2, CellIndices);
                ListPool<Vector3>.Add(CellIndices);
            }

            if(UseUVCoordinates) {
                ManagedMesh.SetUVs(0, UVs);
                ListPool<Vector2>.Add(UVs);
            }

            if(UseUV2Coordinates) {
                ManagedMesh.SetUVs(1, UV2s);
                ListPool<Vector2>.Add(UV2s);
            }

            if(UseColors) {
                ManagedMesh.SetColors(Colors);
                ListPool<Color>.Add(Colors);
            }

            ManagedMesh.RecalculateNormals();

            if(UseCollider) {
                Collider.sharedMesh = ManagedMesh;
            }
        }

        public void AddTriangle(Vector3 vertexOne, Vector3 vertexTwo, Vector3 vertexThree, bool perturbY = false) {
            int vertexIndex = Vertices.Count;

            Vertices.Add(NoiseGenerator.Perturb(vertexOne,   perturbY));
            Vertices.Add(NoiseGenerator.Perturb(vertexTwo,   perturbY));
            Vertices.Add(NoiseGenerator.Perturb(vertexThree, perturbY));

            Triangles.Add(vertexIndex);
            Triangles.Add(vertexIndex + 1);
            Triangles.Add(vertexIndex + 2);
        }

        public void AddTriangleUnperturbed(Vector3 vertexOne, Vector3 vertexTwo, Vector3 vertexThree) {
            int vertexIndex = Vertices.Count;

		    Vertices.Add(vertexOne);
		    Vertices.Add(vertexTwo);
		    Vertices.Add(vertexThree);

		    Triangles.Add(vertexIndex);
		    Triangles.Add(vertexIndex + 1);
		    Triangles.Add(vertexIndex + 2);
        }

        public void AddQuad(Vector3 bottomLeft, Vector3 bottomRight, Vector3 topLeft, Vector3 topRight, bool perturbY = false) {
            int vertexIndex = Vertices.Count;

            Vertices.Add(NoiseGenerator.Perturb(bottomLeft,  perturbY));
		    Vertices.Add(NoiseGenerator.Perturb(bottomRight, perturbY));
		    Vertices.Add(NoiseGenerator.Perturb(topLeft,     perturbY));
		    Vertices.Add(NoiseGenerator.Perturb(topRight,    perturbY));

		    Triangles.Add(vertexIndex);
		    Triangles.Add(vertexIndex + 2);
		    Triangles.Add(vertexIndex + 1);
		    Triangles.Add(vertexIndex + 1);
		    Triangles.Add(vertexIndex + 2);
		    Triangles.Add(vertexIndex + 3);
        }

        public void AddQuadUnperturbed(Vector3 bottomLeft, Vector3 bottomRight, Vector3 topLeft, Vector3 topRight) {
            int vertexIndex = Vertices.Count;

            Vertices.Add(bottomLeft);
		    Vertices.Add(bottomRight);
		    Vertices.Add(topLeft);
		    Vertices.Add(topRight);

		    Triangles.Add(vertexIndex);
		    Triangles.Add(vertexIndex + 2);
		    Triangles.Add(vertexIndex + 1);
		    Triangles.Add(vertexIndex + 1);
		    Triangles.Add(vertexIndex + 2);
		    Triangles.Add(vertexIndex + 3);
        }

        public void AddTriangleUV(Vector2 uv1, Vector2 uv2, Vector2 uv3) {
            UVs.Add(uv1);
            UVs.Add(uv2);
            UVs.Add(uv3);
        }

        public void AddQuadUV(Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4) {
            UVs.Add(uv1);
            UVs.Add(uv2);
            UVs.Add(uv3);
            UVs.Add(uv4);
        }

        public void AddQuadUV(float uMin, float uMax, float vMin, float vMax) {
            UVs.Add(new Vector2(uMin, vMin));
            UVs.Add(new Vector2(uMax, vMin));
            UVs.Add(new Vector2(uMin, vMax));
            UVs.Add(new Vector2(uMax, vMax));
        }

        public void AddTriangleUV2(Vector2 uv1, Vector2 uv2, Vector2 uv3) {
            UV2s.Add(uv1);
            UV2s.Add(uv2);
            UV2s.Add(uv3);
        }

        public void AddQuadUV2(Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4) {
            UV2s.Add(uv1);
            UV2s.Add(uv2);
            UV2s.Add(uv3);
            UV2s.Add(uv4);
        }

        public void AddQuadUV2(float uMin, float uMax, float vMin, float vMax) {
            UV2s.Add(new Vector2(uMin, vMin));
            UV2s.Add(new Vector2(uMax, vMin));
            UV2s.Add(new Vector2(uMin, vMax));
            UV2s.Add(new Vector2(uMax, vMax));
        }

        public void AddTriangleCellData(Vector3 indices, Color weights1, Color weights2, Color weights3) {
            CellIndices.Add(indices);
            CellIndices.Add(indices);
            CellIndices.Add(indices);

            CellWeights.Add(weights1);
            CellWeights.Add(weights2);
            CellWeights.Add(weights3);
        }

        public void AddTriangleCellData(Vector3 indices, Color weights) {
            AddTriangleCellData(indices, weights, weights, weights);
        }

        public void AddQuadCellData(Vector3 indices, Color weights1, Color weights2, Color weights3, Color weights4) {
            CellIndices.Add(indices);
            CellIndices.Add(indices);
            CellIndices.Add(indices);
            CellIndices.Add(indices);

            CellWeights.Add(weights1);
            CellWeights.Add(weights2);
            CellWeights.Add(weights3);
            CellWeights.Add(weights4);
        }

        public void AddQuadCellData(Vector3 indices, Color weights1, Color weights2) {
            AddQuadCellData(indices, weights1, weights1, weights2, weights2);
        }

        public void AddQuadCellData(Vector3 indices, Color weights) {
            AddQuadCellData(indices, weights, weights, weights, weights);
        }

        public void AddTriangleColor(Color color) {
            Colors.Add(color);
            Colors.Add(color);
            Colors.Add(color);
        }

        public void AddTriangleColor(Color colorOne, Color colorTwo, Color colorThree) {
            Colors.Add(colorOne);
            Colors.Add(colorTwo);
            Colors.Add(colorThree);
        }

        public void AddQuadColor(Color colorOne, Color colorTwo, Color colorThree, Color colorFour) {
            Colors.Add(colorOne);
            Colors.Add(colorTwo);
            Colors.Add(colorThree);
            Colors.Add(colorFour);
        }

        public void AddQuadColor(Color colorOne, Color colorTwo) {
            Colors.Add(colorOne);
            Colors.Add(colorOne);

            Colors.Add(colorTwo);
            Colors.Add(colorTwo);
        }

        public void AddQuadColor(Color color) {
            Colors.Add(color);
            Colors.Add(color);
            Colors.Add(color);
            Colors.Add(color);
        }

        #endregion

    }

}
