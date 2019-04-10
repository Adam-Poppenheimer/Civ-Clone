using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.MapRendering {

    public class TerrainBaker : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private Camera BakingCamera;

        [SerializeField] private LayerMask OcclusionMask;
        [SerializeField] private LayerMask DrawingMask;

        public RenderTexture TerrainTexture { get; private set; }
        public RenderTexture WaterTexture   { get; private set; }




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

            WaterTexture = new RenderTexture(
                width:  Mathf.RoundToInt(bakeData.TexelsPerUnit * RenderConfig.ChunkWidth),
                height: Mathf.RoundToInt(bakeData.TexelsPerUnit * RenderConfig.ChunkHeight),
                depth:  bakeData.Depth,
                format: bakeData.Format,
                readWrite: RenderTextureReadWrite.Default
            );

            WaterTexture.filterMode = FilterMode.Trilinear;
            WaterTexture.wrapMode   = TextureWrapMode.Clamp;

            BakingCamera.orthographic     = true;
            BakingCamera.orthographicSize = RenderConfig.ChunkHeight / 2f;
            BakingCamera.aspect           = RenderConfig.ChunkWidth / RenderConfig.ChunkHeight;

            BakingCamera.enabled = false;

            Vector3 localPos = transform.localPosition;

            localPos.x = RenderConfig.ChunkWidth  / 2f;
            localPos.z = RenderConfig.ChunkHeight / 2f;

            transform.localPosition = localPos;
        }

        public void Bake() {
            var meshesToBake = HexMeshFactory.AllMeshes.Where(mesh => mesh.ShouldBeBaked);
            
            foreach(var mesh in meshesToBake) {
                mesh.SetActive(true);
            }

            BakingCamera.targetTexture = TerrainTexture;

            BakingCamera.cullingMask = OcclusionMask;
            BakingCamera.clearFlags = CameraClearFlags.SolidColor;
            BakingCamera.RenderWithShader(RenderConfig.TerrainBakeOcclusionShader, "RenderType");

            BakingCamera.cullingMask = DrawingMask;
            BakingCamera.clearFlags = CameraClearFlags.Nothing;
            BakingCamera.Render();

            BakingCamera.targetTexture = WaterTexture;

            BakingCamera.clearFlags = CameraClearFlags.SolidColor;
            BakingCamera.Render();

            foreach(var mesh in meshesToBake) {
                mesh.SetActive(false);
            }
        }

        #endregion

    }

}
