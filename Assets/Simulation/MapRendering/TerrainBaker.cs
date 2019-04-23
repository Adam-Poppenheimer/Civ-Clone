using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.Profiling;

using Zenject;
using UniRx;

namespace Assets.Simulation.MapRendering {

    public class TerrainBaker : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private Camera BakingCamera;

        [SerializeField] private LayerMask OcclusionMask;
        [SerializeField] private LayerMask LandDrawingMask;
        [SerializeField] private LayerMask WaterDrawingMask;

        public RenderTexture TerrainTexture { get; private set; }
        public RenderTexture WaterTexture   { get; private set; }

        private Coroutine BakeCoroutine;




        private IMapRenderConfig    RenderConfig;
        private IHexMeshFactory     HexMeshFactory;

        #endregion

        #region instance methods

        [Inject]
        private void InjectDependencies(
            IMapRenderConfig renderConfig, IHexMeshFactory hexMeshFactory, MapRenderingSignals mapRenderingSignals
        ) {
            RenderConfig   = renderConfig;
            HexMeshFactory = hexMeshFactory;

            mapRenderingSignals.FarmlandsTriangulated.Subscribe(unit => Bake());
        }

        #region Unity messages

        private void OnDestroy() {
            if(TerrainTexture != null) {
                TerrainTexture.Release();
                TerrainTexture = null;
            }

            if(WaterTexture != null) {
                WaterTexture.Release();
                WaterTexture = null;
            }
        }

        #endregion

        public void Initialize() {
            Profiler.BeginSample("TerrainBaker.Initialize()");

            if(TerrainTexture != null) {
                TerrainTexture.Release();
            }

            if(WaterTexture != null) {
                WaterTexture.Release();
            }

            var bakeData = RenderConfig.TerrainBakeTextureData;

            TerrainTexture = new RenderTexture(
                width:  Mathf.RoundToInt(bakeData.TexelsPerUnit * RenderConfig.ChunkWidth),
                height: Mathf.RoundToInt(bakeData.TexelsPerUnit * RenderConfig.ChunkHeight),
                depth:  bakeData.Depth,
                format: bakeData.Format,
                readWrite: RenderTextureReadWrite.Default
            );

            TerrainTexture.filterMode = FilterMode.Trilinear;
            TerrainTexture.wrapMode   = TextureWrapMode.Clamp;
            TerrainTexture.useMipMap  = true;

            WaterTexture = new RenderTexture(
                width:  Mathf.RoundToInt(bakeData.TexelsPerUnit * RenderConfig.ChunkWidth),
                height: Mathf.RoundToInt(bakeData.TexelsPerUnit * RenderConfig.ChunkHeight),
                depth:  bakeData.Depth,
                format: bakeData.Format,
                readWrite: RenderTextureReadWrite.Default
            );

            WaterTexture.filterMode = FilterMode.Trilinear;
            WaterTexture.wrapMode   = TextureWrapMode.Clamp;
            WaterTexture.useMipMap  = true;

            float cameraWidth  = RenderConfig.ChunkWidth  + RenderConfig.OuterRadius * 3f;
            float cameraHeight = RenderConfig.ChunkHeight + RenderConfig.OuterRadius * 3f;

            BakingCamera.orthographic     = true;
            BakingCamera.orthographicSize = cameraHeight / 2f;
            BakingCamera.aspect           = cameraWidth / cameraHeight;

            BakingCamera.enabled = false;

            Vector3 localPos = transform.localPosition;

            localPos.x = RenderConfig.ChunkWidth  / 2f;
            localPos.z = RenderConfig.ChunkHeight / 2f;

            transform.localPosition = localPos;

            Profiler.EndSample();
        }

        public void Bake() {
            if(BakeCoroutine == null) {
                BakeCoroutine = StartCoroutine(Bake_Perform());
            }
        }

        private IEnumerator Bake_Perform() {
            yield return new WaitForEndOfFrame();

            Profiler.BeginSample("TerrainBaker.Bake_Perform()");

            var meshesToBake = HexMeshFactory.AllMeshes.Where(mesh => mesh.ShouldBeBaked);
            
            foreach(var mesh in meshesToBake) {
                mesh.SetActive(true);
            }

            BakingCamera.targetTexture = TerrainTexture;

            BakingCamera.cullingMask = OcclusionMask;
            BakingCamera.clearFlags = CameraClearFlags.SolidColor;
            BakingCamera.RenderWithShader(RenderConfig.TerrainBakeOcclusionShader, "RenderType");

            BakingCamera.cullingMask = LandDrawingMask;
            BakingCamera.clearFlags = CameraClearFlags.Nothing;
            BakingCamera.Render();

            BakingCamera.targetTexture = WaterTexture;

            BakingCamera.cullingMask = WaterDrawingMask;
            BakingCamera.clearFlags = CameraClearFlags.SolidColor;
            BakingCamera.Render();

            foreach(var mesh in meshesToBake) {
                mesh.SetActive(false);
            }

            Profiler.EndSample();

            BakeCoroutine = null;
        }

        #endregion

    }

}
