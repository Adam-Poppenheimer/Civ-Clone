﻿using System;
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
        [SerializeField] private bool UseColors;
        [SerializeField] private bool UseUVCoordinates;
        [SerializeField] private bool UseUV2Coordinates;
        [SerializeField] private bool UseTerrainTypes;

        private Mesh ManagedMesh;
        public MeshCollider Collider { get; private set; }

        [NonSerialized] private List<Vector3> Vertices;
        [NonSerialized] private List<int>     Triangles;
        [NonSerialized] private List<Color>   Colors;
        [NonSerialized] private List<Vector2> UVs;
        [NonSerialized] private List<Vector2> UV2s;
        [NonSerialized] private List<Vector3> TerrainTypes;

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

            if(UseColors) {
                Colors = ListPool<Color>.Get();
            }

            if(UseUVCoordinates) {
                UVs = ListPool<Vector2>.Get();
            }

            if(UseUV2Coordinates) {
                UV2s = ListPool<Vector2>.Get();
            }

            if(UseTerrainTypes) {
                TerrainTypes = ListPool<Vector3>.Get();
            }
        }

        public void Apply() {
            ManagedMesh.SetVertices(Vertices);
            ListPool<Vector3>.Add(Vertices);

            ManagedMesh.SetTriangles(Triangles, 0);
            ListPool<int>.Add(Triangles);

            if(UseColors) {
                ManagedMesh.SetColors(Colors);
                ListPool<Color>.Add(Colors);
            }

            if(UseUVCoordinates) {
                ManagedMesh.SetUVs(0, UVs);
                ListPool<Vector2>.Add(UVs);
            }

            if(UseUV2Coordinates) {
                ManagedMesh.SetUVs(1, UV2s);
                ListPool<Vector2>.Add(UV2s);
            }

            if(UseTerrainTypes) {
                ManagedMesh.SetUVs(2, TerrainTypes);
                ListPool<Vector3>.Add(TerrainTypes);
            }

            ManagedMesh.RecalculateNormals();

            if(UseCollider) {
                Collider.sharedMesh = ManagedMesh;
            }
        }

        public void AddTriangle(Vector3 vertexOne, Vector3 vertexTwo, Vector3 vertexThree) {
            int vertexIndex = Vertices.Count;

            Vertices.Add(NoiseGenerator.Perturb(vertexOne));
            Vertices.Add(NoiseGenerator.Perturb(vertexTwo));
            Vertices.Add(NoiseGenerator.Perturb(vertexThree));

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

        public void AddQuad(Vector3 vertexOne, Vector3 vertexTwo, Vector3 vertexThree, Vector3 vertexFour) {
            int vertexIndex = Vertices.Count;

            Vertices.Add(NoiseGenerator.Perturb(vertexOne));
		    Vertices.Add(NoiseGenerator.Perturb(vertexTwo));
		    Vertices.Add(NoiseGenerator.Perturb(vertexThree));
		    Vertices.Add(NoiseGenerator.Perturb(vertexFour));

		    Triangles.Add(vertexIndex);
		    Triangles.Add(vertexIndex + 2);
		    Triangles.Add(vertexIndex + 1);
		    Triangles.Add(vertexIndex + 1);
		    Triangles.Add(vertexIndex + 2);
		    Triangles.Add(vertexIndex + 3);
        }

        public void AddQuadUnperturbed(Vector3 vertexOne, Vector3 vertexTwo, Vector3 vertexThree, Vector3 vertexFour) {
            int vertexIndex = Vertices.Count;

            Vertices.Add(vertexOne);
		    Vertices.Add(vertexTwo);
		    Vertices.Add(vertexThree);
		    Vertices.Add(vertexFour);

		    Triangles.Add(vertexIndex);
		    Triangles.Add(vertexIndex + 2);
		    Triangles.Add(vertexIndex + 1);
		    Triangles.Add(vertexIndex + 1);
		    Triangles.Add(vertexIndex + 2);
		    Triangles.Add(vertexIndex + 3);
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

        public void AddTriangleTerrainTypes(Vector3 types) {
            TerrainTypes.Add(types);
            TerrainTypes.Add(types);
            TerrainTypes.Add(types);
        }

        public void AddQuadTerrainTypes(Vector3 types) {
            TerrainTypes.Add(types);
            TerrainTypes.Add(types);
            TerrainTypes.Add(types);
            TerrainTypes.Add(types);
        }

        #endregion

    }

}
