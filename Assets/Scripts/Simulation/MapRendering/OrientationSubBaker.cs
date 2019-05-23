using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Rendering;

using Zenject;

namespace Assets.Simulation.MapRendering {

    public class OrientationSubBaker : MonoBehaviour {

        #region instance fields and properties

        #region from IOrientationSubBaker

        public Texture2D Texture {
            get { return texture; }
        }
        [SerializeField] private Texture2D texture;

        public bool IsReady { get; private set; }

        #endregion

        [SerializeField] private Camera Camera = null;

        [SerializeField] private RenderTexture RenderTexture;

        private Transform OriginalParent;




        private IMapRenderConfig RenderConfig;

        #endregion

        #region instance methods

        [Inject]
        private void InjectDependencies(IMapRenderConfig renderConfig) {
            RenderConfig = renderConfig;

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

            Camera.orthographic     = true;
            Camera.orthographicSize = cameraHeight / 2f;
            Camera.aspect           = cameraWidth / cameraHeight;
            Camera.targetTexture    = RenderTexture;

            Camera.enabled = false;

            Vector3 localPos = transform.localPosition;

            localPos.x = RenderConfig.ChunkWidth  / 2f;
            localPos.z = RenderConfig.ChunkHeight / 2f;

            transform.localPosition = localPos;

            texture = new Texture2D(
                RenderConfig.OrientationTextureData.Resolution,
                RenderConfig.OrientationTextureData.Resolution,
                TextureFormat.ARGB32, false
            );

            texture.filterMode = FilterMode.Point;
            texture.wrapMode   = TextureWrapMode.Clamp;
            texture.anisoLevel = 0;
        }

        #region Unity messages

        private void OnDestroy() {
            if(RenderTexture != null) {
                Destroy(RenderTexture);
            }

            if(Texture != null) {
                Destroy(Texture);
            }
        }

        #endregion

        #region from IOrientationSubBaker

        public void PerformBakePass(IMapChunk chunk, CameraClearFlags clearFlags, LayerMask cullingMask) {
            IsReady = false;

            OriginalParent = transform.parent;

            Camera.transform.SetParent(chunk.transform, false);

            Camera.clearFlags  = clearFlags;
            Camera.cullingMask = cullingMask;

            Camera.Render();
        }

        public void PerformBakePass(IMapChunk chunk, CameraClearFlags clearFlags, LayerMask cullingMask, Shader replacementShader, string replacementTag) {
            IsReady = false;

            OriginalParent = transform.parent;

            Camera.transform.SetParent(chunk.transform, false);

            Camera.clearFlags  = clearFlags;
            Camera.cullingMask = cullingMask;

            Camera.RenderWithShader(replacementShader, replacementTag);
        }

        public void ReadPixels() {
            AsyncGPUReadback.Request(src: RenderTexture, mipIndex: 0, dstFormat: TextureFormat.ARGB32, callback: request => {
                if(request.hasError) {
                    Debug.LogError("Error reading pixels from the RenderTexture");

                }else {
                    var requestedData = request.GetData<byte>();

                    var rawTexture = Texture.GetRawTextureData<byte>();

                    rawTexture.CopyFrom(requestedData);
                }

                IsReady = true;
            });

            transform.SetParent(OriginalParent, false);
        }

        #endregion

        #endregion

    }

}
