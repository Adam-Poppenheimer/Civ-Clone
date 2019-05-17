using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.Profiling;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public class OrientationBaker : MonoBehaviour, IOrientationBaker {

        #region instance fields and properties

        #region from IOrientationBaker

        public Texture2D OrientationTexture {
            get { return orientationTexture; }
        }
        [SerializeField] private Texture2D orientationTexture;

        public Texture2D WeightsTexture {
            get { return weightsTexture; }
        }
        [SerializeField] private Texture2D weightsTexture;

        public Texture2D DuckTexture {
            get { return duckTexture; }
        }
        [SerializeField] private Texture2D duckTexture;

        #endregion

        [SerializeField] private Camera OrientationCamera = null;

        [SerializeField] private LayerMask OrientationCullingMask   = 0;
        [SerializeField] private LayerMask NormalWeightsCullingMask = 0;
        [SerializeField] private LayerMask RiverWeightsCullingMask  = 0;
        [SerializeField] private LayerMask RiverDuckCullingMask     = 0;

        private IHexMesh OrientationMesh {
            get {
                if(_orientationMesh == null) {
                    _orientationMesh = HexMeshFactory.Create("Orientation Mesh", RenderConfig.OrientationMeshData);

                    _orientationMesh.transform.SetParent(transform, false);
                }

                return _orientationMesh;
            }
        }
        private IHexMesh _orientationMesh;

        private IHexMesh WeightsMesh {
            get {
                if(_weightsMesh == null) {
                    _weightsMesh = HexMeshFactory.Create("Weights Mesh", RenderConfig.WeightsMeshData);

                    _weightsMesh.transform.SetParent(transform, false);
                }

                return _weightsMesh;
            }
        }
        private IHexMesh _weightsMesh;

        private RenderTexture RenderTexture;
        



        private IMapRenderConfig         RenderConfig;
        private IHexMeshFactory          HexMeshFactory;
        private IOrientationTriangulator OrientationTriangulator;
        private IWeightsTriangulator     WeightsTriangulator;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IMapRenderConfig renderConfig, IHexMeshFactory hexMeshFactory,
            IOrientationTriangulator orientationTriangulator, IWeightsTriangulator weightsTriangulator
        ) {
            RenderConfig            = renderConfig;
            HexMeshFactory          = hexMeshFactory;
            OrientationTriangulator = orientationTriangulator;
            WeightsTriangulator     = weightsTriangulator;

            Initialize();
        }

        #region Unity messages

        private void OnDestroy() {
            if(RenderTexture != null) {
                RenderTexture.Release();
                RenderTexture = null;
            }

            Destroy(OrientationTexture);
            Destroy(WeightsTexture);
            Destroy(DuckTexture);
        }

        #endregion

        private void Initialize() {
            if(RenderTexture != null) {
                RenderTexture.Release();
            }

            var textureData = RenderConfig.OrientationTextureData;

            RenderTexture = new RenderTexture(
                width:  textureData.Resolution,
                height: textureData.Resolution,
                depth:  textureData.Depth,
                format: textureData.RenderTextureFormat,
                readWrite: RenderTextureReadWrite.Default
            );

            RenderTexture.filterMode = FilterMode.Point;
            RenderTexture.wrapMode   = TextureWrapMode.Clamp;
            RenderTexture.useMipMap  = false;

            float cameraWidth  = RenderConfig.ChunkWidth;
            float cameraHeight = RenderConfig.ChunkHeight;

            OrientationCamera.orthographic     = true;
            OrientationCamera.orthographicSize = cameraHeight / 2f;
            OrientationCamera.aspect           = cameraWidth / cameraHeight;
            OrientationCamera.targetTexture    = RenderTexture;

            OrientationCamera.enabled = false;

            orientationTexture = BuildTexture();
            weightsTexture     = BuildTexture();
            duckTexture        = BuildTexture();

            Vector3 localPos = OrientationCamera.transform.localPosition;

            localPos.x = RenderConfig.ChunkWidth  / 2f;
            localPos.z = RenderConfig.ChunkHeight / 2f;

            OrientationCamera.transform.localPosition = localPos;
        }

        #region from IOrientationBaker

        public void RenderOrientationFromChunk(IMapChunk chunk) {

            OrientationCamera.transform.SetParent(chunk.transform, false);

            var activeRenderTexture = RenderTexture.active;

            RenderOrientation(chunk, OrientationTexture);
            RenderWeights    (chunk, WeightsTexture);
            RenderRiverDuck  (chunk, DuckTexture);

            RenderTexture.active = activeRenderTexture;

            OrientationCamera.transform.SetParent(null, false);
        }

        #endregion

        private Texture2D BuildTexture() {
            var retval = new Texture2D(
                RenderConfig.OrientationTextureData.Resolution,
                RenderConfig.OrientationTextureData.Resolution,
                TextureFormat.ARGB32, false
            );

            retval.filterMode = FilterMode.Point;
            retval.wrapMode   = TextureWrapMode.Clamp;
            retval.anisoLevel = 0;

            return retval;
        }

        private void RenderOrientation(IMapChunk chunk, Texture2D orientationTexture) {
            OrientationMesh.Clear();

            OrientationMesh.transform.SetParent(chunk.transform, false);

            foreach(var cell in chunk.CenteredCells) {
                OrientationTriangulator.TriangulateOrientation(cell, OrientationMesh);
            }

            foreach(var cell in chunk.OverlappingCells) {
                OrientationTriangulator.TriangulateOrientation(cell, OrientationMesh);
            }

            OrientationMesh.Apply();

            OrientationCamera.clearFlags  = CameraClearFlags.SolidColor;
            OrientationCamera.cullingMask = OrientationCullingMask;

            OrientationCamera.Render();

            RenderTexture.active = RenderTexture;

            orientationTexture.ReadPixels(new Rect(0, 0, RenderTexture.width, RenderTexture.height), 0, 0);

            orientationTexture.Apply();
        }

        private void RenderWeights(IMapChunk chunk, Texture2D weightsTexture) {
            WeightsMesh.Clear();

            WeightsMesh.transform.SetParent(chunk.transform, false);

            foreach(var cell in chunk.CenteredCells) {
                WeightsTriangulator.TriangulateCellWeights(cell, WeightsMesh);
            }

            foreach(var cell in chunk.OverlappingCells) {
                WeightsTriangulator.TriangulateCellWeights(cell, WeightsMesh);
            }

            WeightsMesh.Apply();

            OrientationCamera.clearFlags  = CameraClearFlags.SolidColor;
            OrientationCamera.cullingMask = NormalWeightsCullingMask;

            OrientationCamera.Render();

            OrientationCamera.clearFlags  = CameraClearFlags.Nothing;
            OrientationCamera.cullingMask = RiverWeightsCullingMask;

            OrientationCamera.RenderWithShader(RenderConfig.RiverWeightShader, "RenderType");

            RenderTexture.active = RenderTexture;

            weightsTexture.ReadPixels(new Rect(0, 0, RenderTexture.width, RenderTexture.height), 0, 0);

            weightsTexture.Apply();
        }

        private void RenderRiverDuck(IMapChunk chunk, Texture2D duckTexture) {
            OrientationCamera.clearFlags  = CameraClearFlags.SolidColor;
            OrientationCamera.cullingMask = RiverDuckCullingMask;

            OrientationCamera.Render();

            RenderTexture.active = RenderTexture;

            duckTexture.ReadPixels(new Rect(0, 0, RenderTexture.width, RenderTexture.height), 0, 0);

            duckTexture.Apply();
        }

        #endregion

    }

}
