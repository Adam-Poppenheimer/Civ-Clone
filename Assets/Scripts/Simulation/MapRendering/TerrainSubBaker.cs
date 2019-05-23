using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Rendering;

using Zenject;

namespace Assets.Simulation.MapRendering {

    public class TerrainSubBaker : MonoBehaviour {

        #region instance fields and properties

        public bool IsReady { get; private set; }

        private RenderTexture HighResRenderTexture;
        private RenderTexture MediumResRenderTexture;
        private RenderTexture LowResRenderTexture;

        [SerializeField] private Camera BakingCamera = null;

        private Transform OriginalParent;

        #endregion




        
        private IMapRenderConfig   RenderConfig;
        private ITerrainBakeConfig TerrainBakeConfig;

        #region instance methods

        [Inject]
        private void InjectDependencies(IMapRenderConfig renderConfig, ITerrainBakeConfig terrainBakeConfig) {
            RenderConfig      = renderConfig;
            TerrainBakeConfig = terrainBakeConfig;

            HighResRenderTexture   = BuildRenderTexture(TerrainBakeConfig.BakeTextureDimensionsHighRes);
            MediumResRenderTexture = BuildRenderTexture(TerrainBakeConfig.BakeTextureDimensionsMediumRes);
            LowResRenderTexture    = BuildRenderTexture(TerrainBakeConfig.BakeTextureDimensionsLowRes);

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

        }

        private RenderTexture BuildRenderTexture(Vector2 dimensions) {
            var retval = new RenderTexture(
                width:  Mathf.RoundToInt(dimensions.x),
                height: Mathf.RoundToInt(dimensions.y),
                depth:  TerrainBakeConfig.RenderTextureDepth,
                format: TerrainBakeConfig.RenderTextureFormat,
                readWrite: RenderTextureReadWrite.Default
            );

            retval.filterMode = FilterMode.Trilinear;
            retval.wrapMode   = TextureWrapMode.Clamp;
            retval.useMipMap  = false;

            return retval;
        }

        #region Unity messages

        private void OnDestroy() {
            if(HighResRenderTexture   != null) { Destroy(HighResRenderTexture  ); }
            if(MediumResRenderTexture != null) { Destroy(MediumResRenderTexture); }
            if(LowResRenderTexture    != null) { Destroy(LowResRenderTexture   ); }
        }

        #endregion

        public void PerformBakePass(
            IMapChunk chunk, Texture2D texture, CameraClearFlags clearFlags, LayerMask cullingMask,
            Shader replacementShader = null, string replacementTag = ""
        ) {
            IsReady = false;

            OriginalParent = transform.parent;

            transform.SetParent(chunk.transform, false);

            var renderTarget = GetRenderTarget(texture);

            BakingCamera.targetTexture = renderTarget;
            BakingCamera.clearFlags    = clearFlags;
            BakingCamera.cullingMask   = cullingMask;

            if(replacementShader != null) {
                BakingCamera.RenderWithShader(replacementShader, replacementTag);

            }else {
                BakingCamera.Render();
            }
        }

        public void ReadPixels(Texture2D texture, Action<Texture2D> postReadAction) {
            var renderTarget = GetRenderTarget(texture);

            AsyncGPUReadback.Request(src: renderTarget, mipIndex: 0, dstFormat: TextureFormat.ARGB32, callback: request => {
                if(request.hasError) {
                    Debug.LogError("Error reading pixels from the RenderTexture");

                }else {
                    var requestedData = request.GetData<byte>();

                    var rawTexture = texture.GetRawTextureData<byte>();

                    rawTexture.CopyFrom(requestedData);

                    texture.Apply();
                    texture.Compress(false);

                    postReadAction(texture);
                }

                IsReady = true;
            });
        }

        private RenderTexture GetRenderTarget(Texture2D texture) {
            if( texture.width  == TerrainBakeConfig.BakeTextureDimensionsHighRes.x &&
                texture.height == TerrainBakeConfig.BakeTextureDimensionsHighRes.y
            ) {
                return HighResRenderTexture;

            }else if(
                texture.width  == TerrainBakeConfig.BakeTextureDimensionsMediumRes.x &&
                texture.height == TerrainBakeConfig.BakeTextureDimensionsMediumRes.y
            ) {
                return MediumResRenderTexture;

            }else {
                return LowResRenderTexture;
            }
        }

        #endregion

    }

}
