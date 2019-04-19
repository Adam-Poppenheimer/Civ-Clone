using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;

using Assets.Util;

namespace Assets.Simulation.MapRendering {

    public class HexMesh : MonoBehaviour, IHexMesh {

        #region internal types

        public class Pool : MonoMemoryPool<string, HexMeshData, HexMesh> {

            protected override void OnDespawned(HexMesh item) {
                item.Data = null;

                item.Clear();

                base.OnDespawned(item);
            }

            protected override void Reinitialize(string name, HexMeshData data, HexMesh item) {
                item.name = name;
                item.Data = data;

                item.Clear();

                item.OverriddenMaterial = null;
            }

        }

        #endregion

        #region instance fields and properties

        #region from IHexMesh

        public bool ShouldBeBaked {
            get { return Data.ShouldBeBaked; }
        }

        #endregion

        private HexMeshData Data {
            get { return _data; }
            set {
                _data = value;

                if(_data != null) {
                    gameObject.layer = _data.Layer;
                }
            }
        }
        [SerializeField] private HexMeshData _data;

        [NonSerialized] private List<List<Vector3>> VertexLists     = new List<List<Vector3>>();
        [NonSerialized] private List<List<int>>     TriangleLists   = new List<List<int>>();
        [NonSerialized] private List<List<Color>>   CellWeightLists = new List<List<Color>>();
        [NonSerialized] private List<List<Vector2>> UVLists         = new List<List<Vector2>>();
        [NonSerialized] private List<List<Vector2>> UV2Lists        = new List<List<Vector2>>();
        [NonSerialized] private List<List<Vector4>> UV3Lists        = new List<List<Vector4>>();
        [NonSerialized] private List<List<Vector3>> CellIndexLists  = new List<List<Vector3>>();
        [NonSerialized] private List<List<Color>>   ColorLists      = new List<List<Color>>();

        private List<HexSubMesh> SubMeshes = new List<HexSubMesh>();

        private Material OverriddenMaterial = null;



        
        private IMeshWelder                               MeshWelder;
        private IMemoryPool<HexRenderingData, HexSubMesh> SubMeshPool;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IMeshWelder meshWelder, IMemoryPool<HexRenderingData, HexSubMesh> subMeshPool
        ) {
            MeshWelder  = meshWelder;
            SubMeshPool = subMeshPool;
        }

        #region Unity messages

        private void OnDestroy() {
            Clear();
        }

        #endregion

        #region from IHexMesh

        public void Clear() {
            foreach(var subMesh in SubMeshes) {
                SubMeshPool.Despawn(subMesh);
            }

            SubMeshes.Clear();

            if(Data != null) {
                AddNewVertexCollection();
            }else {
                VertexLists    .ForEach(list => ListPool<Vector3>.Add(list));
                TriangleLists  .ForEach(list => ListPool<int>    .Add(list));
                CellWeightLists.ForEach(list => ListPool<Color>  .Add(list));
                CellIndexLists .ForEach(list => ListPool<Vector3>.Add(list));
                UVLists        .ForEach(list => ListPool<Vector2>.Add(list));
                UV2Lists       .ForEach(list => ListPool<Vector2>.Add(list));
                UV3Lists       .ForEach(list => ListPool<Vector4>.Add(list));
                ColorLists     .ForEach(list => ListPool<Color>  .Add(list));

                VertexLists    .Clear();
                TriangleLists  .Clear();
                CellWeightLists.Clear();
                CellIndexLists .Clear();
                UVLists        .Clear();
                UV2Lists       .Clear();
                UV3Lists       .Clear();
                ColorLists     .Clear();
            }
        }

        public void Apply() {
            for(int i = 0; i < VertexLists.Count; i++) {
                List<Vector3> vertexList = VertexLists[i];

                var newSubMesh = SubMeshPool.Spawn(Data.RenderingData);

                newSubMesh.gameObject.layer = gameObject.layer;
                newSubMesh.gameObject.name = string.Format("{0} Submesh", gameObject.name);

                newSubMesh.transform.SetParent(transform, false);

                newSubMesh.UseCollider = Data.UseCollider;

                newSubMesh.Mesh.SetVertices(vertexList);
                ListPool<Vector3>.Add(vertexList);

                newSubMesh.Mesh.SetTriangles(TriangleLists[i], 0);
                ListPool<int>.Add(TriangleLists[i]);

                if(Data.UseCellData) {
                    newSubMesh.Mesh.SetColors(CellWeightLists[i]);
                    ListPool<Color>.Add(CellWeightLists[i]);

                    newSubMesh.Mesh.SetUVs(3, CellIndexLists[i]);
                    ListPool<Vector3>.Add(CellIndexLists[i]);
                }

                if(Data.UseUVCoordinates) {
                    newSubMesh.Mesh.SetUVs(0, UVLists[i]);
                    ListPool<Vector2>.Add(UVLists[i]);
                }

                if(Data.UseUV2Coordinates) {
                    newSubMesh.Mesh.SetUVs(1, UV2Lists[i]);
                    ListPool<Vector2>.Add(UV2Lists[i]);
                }

                if(Data.UseUV3Coordinates) {
                    newSubMesh.Mesh.SetUVs(2, UV3Lists[i]);
                    ListPool<Vector4>.Add(UV3Lists[i]);
                }

                if(Data.UseColors) {
                    newSubMesh.Mesh.SetColors(ColorLists[i]);
                    ListPool<Color>.Add(ColorLists[i]);
                }

                if(Data.WeldMesh) {
                    MeshWelder.Weld(newSubMesh.Mesh);
                }
                
                newSubMesh.Mesh.RecalculateNormals();

                if(OverriddenMaterial != null) {
                    newSubMesh.OverrideMaterial(OverriddenMaterial);
                }

                SubMeshes.Add(newSubMesh);
            }

            VertexLists    .Clear();
            TriangleLists  .Clear();
            CellWeightLists.Clear();
            CellIndexLists .Clear();
            UVLists        .Clear();
            UV2Lists       .Clear();
            UV3Lists       .Clear();
            ColorLists     .Clear();

            gameObject.SetActive(!Data.ShouldBeBaked);
        }

        public void AddTriangle(Vector3 vertexOne, Vector3 vertexTwo, Vector3 vertexThree) {
            var activeVertices = VertexLists.Last();

            if(activeVertices.Count >= 65531) {
                activeVertices = AddNewVertexCollection();
            }

            int vertexIndex = activeVertices.Count;

            if(Data.ConvertVerticesToWorld) {
                activeVertices.Add(transform.InverseTransformPoint(vertexOne  ));
		        activeVertices.Add(transform.InverseTransformPoint(vertexTwo  ));
		        activeVertices.Add(transform.InverseTransformPoint(vertexThree));
            }else {
                activeVertices.Add(vertexOne);
		        activeVertices.Add(vertexTwo);
		        activeVertices.Add(vertexThree);
            }		    

            var activeTriangles = TriangleLists.Last();

		    activeTriangles.Add(vertexIndex);
		    activeTriangles.Add(vertexIndex + 1);
		    activeTriangles.Add(vertexIndex + 2);
        }

        public void AddQuad(Vector3 bottomLeft, Vector3 bottomRight, Vector3 topLeft, Vector3 topRight) {
            var activeVertices = VertexLists.Last();

            if(activeVertices.Count >= 65530) {
                activeVertices = AddNewVertexCollection();
            }

            int vertexIndex = activeVertices.Count;

            if(Data.ConvertVerticesToWorld) {
                activeVertices.Add(transform.InverseTransformPoint(bottomLeft ));
                activeVertices.Add(transform.InverseTransformPoint(bottomRight));
                activeVertices.Add(transform.InverseTransformPoint(topLeft    ));
                activeVertices.Add(transform.InverseTransformPoint(topRight   ));
            }else {
                activeVertices.Add(bottomLeft);
		        activeVertices.Add(bottomRight);
		        activeVertices.Add(topLeft);
		        activeVertices.Add(topRight);
            }

            var activeTriangles = TriangleLists.Last();

		    activeTriangles.Add(vertexIndex);
		    activeTriangles.Add(vertexIndex + 2);
		    activeTriangles.Add(vertexIndex + 1);
		    activeTriangles.Add(vertexIndex + 1);
		    activeTriangles.Add(vertexIndex + 2);
		    activeTriangles.Add(vertexIndex + 3);
        }

        public void AddTriangleUV(Vector2 uv1, Vector2 uv2, Vector2 uv3) {
            var activeUV = UVLists.Last();

            activeUV.Add(uv1);
            activeUV.Add(uv2);
            activeUV.Add(uv3);
        }

        public void AddQuadUV(Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4) {
            var activeUV = UVLists.Last();

            activeUV.Add(uv1);
            activeUV.Add(uv2);
            activeUV.Add(uv3);
            activeUV.Add(uv4);
        }

        public void AddQuadUV(float uMin, float uMax, float vMin, float vMax) {
            var activeUV = UVLists.Last();

            activeUV.Add(new Vector2(uMin, vMin));
            activeUV.Add(new Vector2(uMax, vMin));
            activeUV.Add(new Vector2(uMin, vMax));
            activeUV.Add(new Vector2(uMax, vMax));
        }

        public void AddTriangleUV2(Vector2 uv1, Vector2 uv2, Vector2 uv3) {
            var activeUV2 = UV2Lists.Last();

            activeUV2.Add(uv1);
            activeUV2.Add(uv2);
            activeUV2.Add(uv3);
        }

        public void AddQuadUV2(Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4) {
            var activeUV2 = UV2Lists.Last();

            activeUV2.Add(uv1);
            activeUV2.Add(uv2);
            activeUV2.Add(uv3);
            activeUV2.Add(uv4);
        }

        public void AddQuadUV2(float uMin, float uMax, float vMin, float vMax) {
            var activeUV2 = UV2Lists.Last();

            activeUV2.Add(new Vector2(uMin, vMin));
            activeUV2.Add(new Vector2(uMax, vMin));
            activeUV2.Add(new Vector2(uMin, vMax));
            activeUV2.Add(new Vector2(uMax, vMax));
        }

        public void AddTriangleUV3(Vector4 uv1, Vector4 uv2, Vector4 uv3) {
            var activeUV3 = UV3Lists.Last();

            activeUV3.Add(uv1);
            activeUV3.Add(uv2);
            activeUV3.Add(uv3);
        }

        public void AddQuadUV3(Vector4 uv1, Vector4 uv2, Vector4 uv3, Vector4 uv4) {
            var activeUV3 = UV3Lists.Last();

            activeUV3.Add(uv1);
            activeUV3.Add(uv2);
            activeUV3.Add(uv3);
            activeUV3.Add(uv4);
        }

        public void AddTriangleCellData(Vector3 indices, Color weights1, Color weights2, Color weights3) {
            var activeIndices = CellIndexLists.Last();

            activeIndices.Add(indices);
            activeIndices.Add(indices);
            activeIndices.Add(indices);

            var activeWeights = CellWeightLists.Last();

            activeWeights.Add(weights1);
            activeWeights.Add(weights2);
            activeWeights.Add(weights3);
        }

        public void AddTriangleCellData(Vector3 indices, Color weights) {
            AddTriangleCellData(indices, weights, weights, weights);
        }

        public void AddQuadCellData(Vector3 indices, Color weights1, Color weights2, Color weights3, Color weights4) {
            var activeIndices = CellIndexLists.Last();

            activeIndices.Add(indices);
            activeIndices.Add(indices);
            activeIndices.Add(indices);
            activeIndices.Add(indices);

            var activeWeights = CellWeightLists.Last();

            activeWeights.Add(weights1);
            activeWeights.Add(weights2);
            activeWeights.Add(weights3);
            activeWeights.Add(weights4);
        }

        public void AddQuadCellData(Vector3 indices, Color weights1, Color weights2) {
            AddQuadCellData(indices, weights1, weights1, weights2, weights2);
        }

        public void AddQuadCellData(Vector3 indices, Color weights) {
            AddQuadCellData(indices, weights, weights, weights, weights);
        }

        public void AddTriangleColor(Color color) {
            var activeColors = ColorLists.Last();

            activeColors.Add(color);
            activeColors.Add(color);
            activeColors.Add(color);
        }

        public void AddTriangleColor(Color colorOne, Color colorTwo, Color colorThree) {
            var activeColors = ColorLists.Last();

            activeColors.Add(colorOne);
            activeColors.Add(colorTwo);
            activeColors.Add(colorThree);
        }

        public void AddQuadColor(Color colorOne, Color colorTwo, Color colorThree, Color colorFour) {
            var activeColors = ColorLists.Last();

            activeColors.Add(colorOne);
            activeColors.Add(colorTwo);
            activeColors.Add(colorThree);
            activeColors.Add(colorFour);
        }

        public void AddQuadColor(Color colorOne, Color colorTwo) {
            var activeColors = ColorLists.Last();

            activeColors.Add(colorOne);
            activeColors.Add(colorOne);

            activeColors.Add(colorTwo);
            activeColors.Add(colorTwo);
        }

        public void AddQuadColor(Color color) {
            var activeColors = ColorLists.Last();

            activeColors.Add(color);
            activeColors.Add(color);
            activeColors.Add(color);
            activeColors.Add(color);
        }

        public void SetActive(bool isActive) {
            gameObject.SetActive(isActive);
        }

        public void OverrideMaterial(Material newMaterial) {
            OverriddenMaterial = newMaterial;

            foreach(var mesh in SubMeshes) {
                mesh.OverrideMaterial(OverriddenMaterial);
            }
        }

        #endregion

        private List<Vector3> AddNewVertexCollection() {
            var newVertices = ListPool<Vector3>.Get();

            VertexLists  .Add(newVertices);
            TriangleLists.Add(ListPool<int>.Get());

            if(Data.UseCellData) {
                CellWeightLists.Add(ListPool<Color>  .Get());
                CellIndexLists .Add(ListPool<Vector3>.Get());
            }

            if(Data.UseUVCoordinates) {
                UVLists.Add(ListPool<Vector2>.Get());
            }

            if(Data.UseUV2Coordinates) {
                UV2Lists.Add(ListPool<Vector2>.Get());
            }

            if(Data.UseUV3Coordinates) {
                UV3Lists.Add(ListPool<Vector4>.Get());
            }

            if(Data.UseColors) {
                ColorLists.Add(ListPool<Color>.Get());
            }

            return newVertices;
        }

        #endregion

    }

}