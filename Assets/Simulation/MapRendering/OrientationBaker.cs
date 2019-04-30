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

        [SerializeField] private Camera OrientationCamera;

        [SerializeField] private LayerMask OrientationCullingMask;
        [SerializeField] private LayerMask NormalWeightsCullingMask;
        [SerializeField] private LayerMask RiverWeightsCullingMask;

        private RenderTexture RenderTexture;



        private IMapRenderConfig RenderConfig;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(IMapRenderConfig renderConfig) {
            RenderConfig   = renderConfig;

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

            var textureData = RenderConfig.OrientationTextureData;

            RenderTexture = new RenderTexture(
                width:  Mathf.RoundToInt(textureData.TexelsPerUnit * RenderConfig.ChunkWidth),
                height: Mathf.RoundToInt(textureData.TexelsPerUnit * RenderConfig.ChunkHeight),
                depth:  textureData.Depth,
                format: textureData.Format,
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

            Vector3 localPos = OrientationCamera.transform.localPosition;

            localPos.x = RenderConfig.ChunkWidth  / 2f;
            localPos.z = RenderConfig.ChunkHeight / 2f;

            OrientationCamera.transform.localPosition = localPos;
        }

        #region from IOrientationBaker

        public void RenderOrientationFromMesh(Texture2D orientationTexture, Texture2D weightsTexture, Transform chunkTransform) {
            StartCoroutine(RenderOrientationFromMesh_Perform(orientationTexture, weightsTexture, chunkTransform));
        }

        #endregion

        private IEnumerator RenderOrientationFromMesh_Perform(
            Texture2D orientationTexture, Texture2D weightsTexture, Transform chunkTransform
        ) {
            yield return new WaitForEndOfFrame();

            Profiler.BeginSample("Orientation and Weight Mesh Rendering");

            OrientationCamera.transform.SetParent(chunkTransform, false);

            var activeRenderTexture = RenderTexture.active;

            RenderOrientation(orientationTexture);
            RenderWeights(weightsTexture);

            RenderTexture.active = activeRenderTexture;

            OrientationCamera.transform.SetParent(null, false);

            Profiler.EndSample();
        }

        private void RenderOrientation(Texture2D orientationTexture) {
            OrientationCamera.clearFlags  = CameraClearFlags.SolidColor;
            OrientationCamera.cullingMask = OrientationCullingMask;

            OrientationCamera.Render();

            RenderTexture.active = RenderTexture;

            orientationTexture.ReadPixels(new Rect(0, 0, RenderTexture.width, RenderTexture.height), 0, 0);

            orientationTexture.Apply();
        }

        private void RenderWeights(Texture2D weightsTexture) {
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

        #endregion

    }

}
