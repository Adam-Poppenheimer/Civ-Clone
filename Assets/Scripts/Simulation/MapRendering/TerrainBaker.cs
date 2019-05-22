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

        [SerializeField] private Camera BakingCamera  = null;

        [SerializeField] private LayerMask OcclusionMask    = 0;
        [SerializeField] private LayerMask LandDrawingMask  = 0;
        [SerializeField] private LayerMask WaterDrawingMask = 0;

        private RenderTexture RenderTexture_HighRes;
        private RenderTexture RenderTexture_MediumRes;
        private RenderTexture RenderTexture_LowRes;

        private Dictionary<IMapChunk, Coroutine> CoroutineForChunk = new Dictionary<IMapChunk, Coroutine>();




        private IMapRenderConfig    RenderConfig;
        private IBakedElementsLogic BakedElementsLogic;
        private ITerrainBakeConfig  TerrainBakeConfig;

        #endregion

        #region instance methods

        [Inject]
        private void InjectDependencies(
            IMapRenderConfig renderConfig, IBakedElementsLogic bakedElementsLogic,
            ITerrainBakeConfig terrainBakeConfig
        ) {
            RenderConfig       = renderConfig;
            BakedElementsLogic = bakedElementsLogic;
            TerrainBakeConfig  = terrainBakeConfig;

            Initialize();
        }

        #region Unity messages

        private void OnDestroy() {
            if(RenderTexture_HighRes != null) {
                RenderTexture_HighRes.Release();
                RenderTexture_HighRes = null;
            }

            if(RenderTexture_MediumRes != null) {
                RenderTexture_MediumRes.Release();
                RenderTexture_MediumRes = null;
            }

            if(RenderTexture_LowRes != null) {
                RenderTexture_LowRes.Release();
                RenderTexture_LowRes = null;
            }
        }

        #endregion

        private void Initialize() {
            if(RenderTexture_HighRes   != null) { RenderTexture_HighRes  .Release(); }
            if(RenderTexture_MediumRes != null) { RenderTexture_MediumRes.Release(); }
            if(RenderTexture_LowRes    != null) { RenderTexture_LowRes   .Release(); }

            RenderTexture_HighRes   = BuildRenderTexture(TerrainBakeConfig.BakeTextureDimensionsHighRes);
            RenderTexture_MediumRes = BuildRenderTexture(TerrainBakeConfig.BakeTextureDimensionsMediumRes);
            RenderTexture_LowRes    = BuildRenderTexture(TerrainBakeConfig.BakeTextureDimensionsLowRes);

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

        public void BakeIntoChunk(IMapChunk chunk) {
            if(!CoroutineForChunk.ContainsKey(chunk)) {
                CoroutineForChunk[chunk] = StartCoroutine(BakeIntoChunk_Perform(chunk));
            }
        }

        private IEnumerator BakeIntoChunk_Perform(IMapChunk chunk) {
            yield return new WaitForEndOfFrame();

            while(chunk.IsRefreshing) {
                yield return new WaitForEndOfFrame();
            }

            ResetChunkTextures(chunk);

            if(chunk.LandBakeTexture.width == 0 || chunk.LandBakeTexture.height == 0) {
                CoroutineForChunk.Remove(chunk);
                yield break;
            }

            var activeRenderTexture = RenderTexture.active;

            RenderTexture renderTarget;

            if( chunk.LandBakeTexture.width  == TerrainBakeConfig.BakeTextureDimensionsHighRes.x &&
                chunk.LandBakeTexture.height == TerrainBakeConfig.BakeTextureDimensionsHighRes.y
            ) {
                renderTarget = RenderTexture_HighRes;

            }else if(
                chunk.LandBakeTexture.width  == TerrainBakeConfig.BakeTextureDimensionsMediumRes.x &&
                chunk.LandBakeTexture.height == TerrainBakeConfig.BakeTextureDimensionsMediumRes.y
            ) {
                renderTarget = RenderTexture_MediumRes;

            }else {
                renderTarget = RenderTexture_LowRes;
            }

            RenderTexture.active        = renderTarget;
            BakingCamera .targetTexture = renderTarget;

            BakingCamera.transform.SetParent(chunk.transform, false);

            BakingCamera.cullingMask = OcclusionMask;
            BakingCamera.clearFlags = CameraClearFlags.SolidColor;
            BakingCamera.RenderWithShader(TerrainBakeConfig.TerrainBakeOcclusionShader, "RenderType");

            BakingCamera.cullingMask = LandDrawingMask;
            BakingCamera.clearFlags = CameraClearFlags.Nothing;
            BakingCamera.Render();

            chunk.LandBakeTexture.ReadPixels(new Rect(0, 0, renderTarget.width, renderTarget.height), 0, 0);

            chunk.LandBakeTexture.Apply();
            chunk.LandBakeTexture.Compress(false);

            BakingCamera.cullingMask = WaterDrawingMask;
            BakingCamera.clearFlags = CameraClearFlags.SolidColor;
            BakingCamera.Render();

            chunk.WaterBakeTexture.ReadPixels(new Rect(0, 0, renderTarget.width, renderTarget.height), 0, 0);

            chunk.WaterBakeTexture.Apply();
            chunk.WaterBakeTexture.Compress(false);

            RenderTexture.active = activeRenderTexture;

            BakingCamera.transform.SetParent(null, false);

            CoroutineForChunk.Remove(chunk);
        }

        private void ResetChunkTextures(IMapChunk chunk) {
            var oldLand  = chunk.LandBakeTexture;
            var oldWater = chunk.WaterBakeTexture;

            if(oldLand != null) {
                Destroy(oldLand);
            }

            if(oldWater != null) {
                Destroy(oldWater);
            }

            BakedElementFlags bakedElements = BakedElementsLogic.GetBakedElementsInCells(chunk.CenteredCells);
            bakedElements |= BakedElementsLogic.GetBakedElementsInCells(chunk.OverlappingCells);

            Vector2 textureSize = GetBakeTextureDimensionsForElements(bakedElements);

            var newLand = new Texture2D(
                Mathf.RoundToInt(textureSize.x), Mathf.RoundToInt(textureSize.y),
                TextureFormat.ARGB32, false
            );

            newLand.filterMode = FilterMode.Point;
            newLand.wrapMode   = TextureWrapMode.Clamp;
            newLand.anisoLevel = 0;;

            var newWater = new Texture2D(
                Mathf.RoundToInt(textureSize.x), Mathf.RoundToInt(textureSize.y),
                TextureFormat.ARGB32, false
            );

            newWater.filterMode = FilterMode.Point;
            newWater.wrapMode   = TextureWrapMode.Clamp;
            newWater.anisoLevel = 0;

            chunk.LandBakeTexture  = newLand;
            chunk.WaterBakeTexture = newWater;
        }

        private Vector2 GetBakeTextureDimensionsForElements(BakedElementFlags elements) {
            if((elements & TerrainBakeConfig.BakeElementsHighRes) != 0) {
                return TerrainBakeConfig.BakeTextureDimensionsHighRes;

            }else if((elements & TerrainBakeConfig.BakeElementsMediumRes) != 0) {
                return TerrainBakeConfig.BakeTextureDimensionsMediumRes;

            }else if((elements & TerrainBakeConfig.BakeElementsLowRes) != 0) {
                return TerrainBakeConfig.BakeTextureDimensionsLowRes;

            }else {
                return Vector2.zero;
            }
        }

        #endregion

    }

}
