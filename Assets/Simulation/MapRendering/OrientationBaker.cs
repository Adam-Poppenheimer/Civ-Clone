using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public class OrientationBaker : IOrientationBaker {

        #region instance fields and properties

        private RenderTexture OrientationRenderTexture;



        private IMapRenderConfig RenderConfig;
        private Camera           OrientationCamera;
        private MonoBehaviour    CoroutineInvoker;

        #endregion

        #region constructors

        public OrientationBaker(
            IMapRenderConfig renderConfig, [Inject(Id = "Orientation Camera")] Camera orientationCamera,
            [Inject(Id = "Coroutine Invoker")] MonoBehaviour coroutineInvoker
        ) {
            RenderConfig      = renderConfig;
            OrientationCamera = orientationCamera;
            CoroutineInvoker  = coroutineInvoker;

            Initialize();
        }

        #endregion

        #region instance methods

        private void Initialize() {
            var bakeData = RenderConfig.TerrainBakeTextureData;

            OrientationRenderTexture = new RenderTexture(
                width:  Mathf.RoundToInt(bakeData.TexelsPerUnit * RenderConfig.ChunkWidth),
                height: Mathf.RoundToInt(bakeData.TexelsPerUnit * RenderConfig.ChunkHeight),
                depth:  bakeData.Depth,
                format: bakeData.Format,
                readWrite: RenderTextureReadWrite.Default
            );

            OrientationRenderTexture.filterMode = FilterMode.Point;
            OrientationRenderTexture.wrapMode   = TextureWrapMode.Clamp;
            OrientationRenderTexture.useMipMap  = false;

            float cameraWidth  = RenderConfig.ChunkWidth;
            float cameraHeight = RenderConfig.ChunkHeight;

            OrientationCamera.orthographic     = true;
            OrientationCamera.orthographicSize = cameraHeight / 2f;
            OrientationCamera.aspect           = cameraWidth / cameraHeight;
            OrientationCamera.targetTexture    = OrientationRenderTexture;
            OrientationCamera.cullingMask      = RenderConfig.OrientationCullingMask;

            OrientationCamera.enabled = false;

            Vector3 localPos = OrientationCamera.transform.localPosition;

            localPos.x = RenderConfig.ChunkWidth  / 2f;
            localPos.z = RenderConfig.ChunkHeight / 2f;

            OrientationCamera.transform.localPosition = localPos;
        }

        #region from IOrientationBaker

        public void RenderOrientationFromMesh(Texture2D orientationTexture, Transform chunkTransform) {
            CoroutineInvoker.StartCoroutine(RenderOrientationFromMesh_Execute(orientationTexture, chunkTransform));
        }

        private IEnumerator RenderOrientationFromMesh_Execute(Texture2D orientationTexture, Transform chunkTransform) {
            yield return new WaitForEndOfFrame();

            OrientationCamera.transform.SetParent(chunkTransform, false);

            OrientationCamera.Render();

            var activeRenderTexture = RenderTexture.active;

            RenderTexture.active = OrientationRenderTexture;

            orientationTexture.ReadPixels(new Rect(0, 0, OrientationRenderTexture.width, OrientationRenderTexture.height), 0, 0);

            orientationTexture.Apply();

            RenderTexture.active = activeRenderTexture;
        }

        #endregion

        #endregion

    }

}
