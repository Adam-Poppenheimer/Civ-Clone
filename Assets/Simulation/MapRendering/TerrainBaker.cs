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

    public class TerrainBaker : MonoBehaviour, ITerrainBaker {

        #region instance fields and properties

        [SerializeField] private Camera BakingCamera;

        [SerializeField] private LayerMask OcclusionMask;
        [SerializeField] private LayerMask LandDrawingMask;
        [SerializeField] private LayerMask WaterDrawingMask;

        private RenderTexture RenderTexture;




        private IMapRenderConfig RenderConfig;
        private IHexMeshFactory  HexMeshFactory;

        #endregion

        #region instance methods

        [Inject]
        private void InjectDependencies(
            IMapRenderConfig renderConfig, IHexMeshFactory hexMeshFactory
        ) {
            RenderConfig   = renderConfig;
            HexMeshFactory = hexMeshFactory;

            Initialize();
        }

        #region Unity messages

        private void OnDestroy() {
            if(RenderTexture != null) {
                RenderTexture.Release();
                RenderTexture = null;
            }
        }

        #endregion

        private void Initialize() {
            if(RenderTexture != null) {
                RenderTexture.Release();
            }

            var bakeData = RenderConfig.TerrainBakeTextureData;

            RenderTexture = new RenderTexture(
                width:  Mathf.RoundToInt(bakeData.TexelsPerUnit * RenderConfig.ChunkWidth),
                height: Mathf.RoundToInt(bakeData.TexelsPerUnit * RenderConfig.ChunkHeight),
                depth:  bakeData.Depth,
                format: bakeData.Format,
                readWrite: RenderTextureReadWrite.Default
            );

            RenderTexture.filterMode = FilterMode.Trilinear;
            RenderTexture.wrapMode   = TextureWrapMode.Clamp;
            RenderTexture.useMipMap  = false;

            float cameraWidth  = RenderConfig.ChunkWidth  + RenderConfig.OuterRadius * 3f;
            float cameraHeight = RenderConfig.ChunkHeight + RenderConfig.OuterRadius * 3f;

            BakingCamera.orthographic     = true;
            BakingCamera.orthographicSize = cameraHeight / 2f;
            BakingCamera.aspect           = cameraWidth / cameraHeight;
            BakingCamera.targetTexture    = RenderTexture;

            BakingCamera.enabled = false;

            Vector3 localPos = transform.localPosition;

            localPos.x = RenderConfig.ChunkWidth  / 2f;
            localPos.z = RenderConfig.ChunkHeight / 2f;

            transform.localPosition = localPos;
        }

        public void BakeIntoTextures(Texture2D landTexture, Texture2D waterTexture, Transform chunkTransform) {
            StartCoroutine(BakeIntoTexture_Perform(landTexture, waterTexture, chunkTransform));
        }

        private IEnumerator BakeIntoTexture_Perform(Texture2D landTexture, Texture2D waterTexture, Transform chunkTransform) {
            yield return new WaitForEndOfFrame();

            var activeRenderTexture = RenderTexture.active;

            RenderTexture.active = RenderTexture;

            Profiler.BeginSample("TerrainBaker.BakeIntoTexture_Perform()");

            var meshesToBake = HexMeshFactory.AllMeshes.Where(mesh => mesh.ShouldBeBaked);
            
            foreach(var mesh in meshesToBake) {
                mesh.SetActive(true);
            }

            BakingCamera.transform.SetParent(chunkTransform, false);

            BakingCamera.cullingMask = OcclusionMask;
            BakingCamera.clearFlags = CameraClearFlags.SolidColor;
            BakingCamera.RenderWithShader(RenderConfig.TerrainBakeOcclusionShader, "RenderType");

            BakingCamera.cullingMask = LandDrawingMask;
            BakingCamera.clearFlags = CameraClearFlags.Nothing;
            BakingCamera.Render();

            landTexture.ReadPixels(new Rect(0, 0, RenderTexture.width, RenderTexture.height), 0, 0);

            landTexture.Apply();

            BakingCamera.cullingMask = WaterDrawingMask;
            BakingCamera.clearFlags = CameraClearFlags.SolidColor;
            BakingCamera.Render();

            waterTexture.ReadPixels(new Rect(0, 0, RenderTexture.width, RenderTexture.height), 0, 0);

            waterTexture.Apply();

            foreach(var mesh in meshesToBake) {
                mesh.SetActive(false);
            }

            RenderTexture.active = activeRenderTexture;

            BakingCamera.transform.SetParent(null, false);

            Profiler.EndSample();
        }

        #endregion

    }

}
