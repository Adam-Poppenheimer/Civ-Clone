using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapRendering {

    public class MapChunk : MonoBehaviour, IMapChunk {

        #region static fields and properties

        private static Coroutine RefreshRiversCoroutine;

        #endregion

        #region instance fields and properties

        public IEnumerable<IHexCell> Cells {
            get { return cells; }
        }
        private List<IHexCell> cells = new List<IHexCell>();

        private Terrain Terrain;

        private Coroutine RefreshAlphamapCoroutine;
        private Coroutine RefreshHeightmapCoroutine;
        private Coroutine RefreshWaterCoroutine;
        private Coroutine RefreshFeatureCoroutine;

        [SerializeField] private HexMesh StandingWater;




        private ITerrainAlphamapLogic AlphamapLogic;
        private ITerrainHeightLogic   HeightLogic;
        private IMapRenderConfig      RenderConfig;
        private IWaterTriangulator    WaterTriangulator;
        private IHexFeatureManager    HexFeatureManager;
        private IRiverTriangulator    RiverTriangulator;

        #endregion

        #region instance methods

        [Inject]
        private void InjectDependencies(
            ITerrainAlphamapLogic alphamapLogic, ITerrainHeightLogic heightLogic,
            IMapRenderConfig renderConfig, IWaterTriangulator waterTriangulator,
            IHexFeatureManager hexFeatureManager, IRiverTriangulator riverTriangulator
        ) {
            AlphamapLogic      = alphamapLogic;
            HeightLogic        = heightLogic;
            RenderConfig       = renderConfig;
            WaterTriangulator  = waterTriangulator;
            HexFeatureManager  = hexFeatureManager;
            RiverTriangulator  = riverTriangulator;
        }

        #region from IMapChunk

        public void AttachCell(IHexCell cell) {
            cells.Add(cell);
        }

        public void InitializeTerrain(Vector3 position, float width, float height) {
            var terrainData = BuildTerrainData(width, height);

            var terrainGameObject = Terrain.CreateTerrainGameObject(terrainData);

            terrainGameObject.layer = LayerMask.NameToLayer("Terrain");

            terrainGameObject.transform.SetParent(transform, false);

            Terrain = terrainGameObject.GetComponent<Terrain>();

            transform.position = position;

            Terrain.castShadows = false;

            Terrain.Flush();
        }

        public void RefreshAlphamap() {
            if(RefreshAlphamapCoroutine == null) {
                RefreshAlphamapCoroutine = StartCoroutine(RefreshAlphamap_Perform());
            }
        }

        public void RefreshHeightmap() {
            if(RefreshHeightmapCoroutine == null) {
                RefreshHeightmapCoroutine = StartCoroutine(RefreshHeightmap_Perform());
            }
        }

        public void RefreshWater() {
            if(RefreshWaterCoroutine == null) {
                RefreshWaterCoroutine = StartCoroutine(RefreshWater_Perform());
            }
        }

        public void RefreshFeatures() {
            if(RefreshFeatureCoroutine == null) {
                RefreshFeatureCoroutine = StartCoroutine(RefreshFeatures_Perform());
            }
        }

        public void RefreshAll() {
            RefreshAlphamap();
            RefreshHeightmap();
            RefreshWater();
            RefreshFeatures();

            if(RefreshRiversCoroutine == null) {
                RefreshRiversCoroutine = StartCoroutine(RefreshRivers_Perform());
            }
        }

        public bool DoesCellOverlapChunk(IHexCell cell) {
            return RenderConfig.Corners.Any(corner => IsOnTerrain2D(corner + cell.AbsolutePosition));
        }

        public bool IsOnTerrain2D(Vector3 position3D) {
            float xMin = transform.position.x;
            float xMax = transform.position.x + Terrain.terrainData.size.x;
            float zMin = transform.position.z;
            float zMax = transform.position.z + Terrain.terrainData.size.z;

            return position3D.x >= xMin && position3D.x <= xMax
                && position3D.z >= zMin && position3D.z <= zMax;
        }

        #endregion

        private IEnumerator RefreshAlphamap_Perform() {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            var terrainData = Terrain.terrainData;

            int mapWidth  = terrainData.alphamapWidth;
            int mapHeight = terrainData.alphamapHeight;

            float[,,] alphaMaps = terrainData.GetAlphamaps(0, 0, mapWidth, mapHeight);

            for(int height = 0; height < mapHeight; height++) {
                for(int width = 0; width < mapWidth; width++) {
                    //For some reason, terrainData seems to index its points
                    //as (y, x) rather than the more traditional (x, y), so
                    //the worldX needs to be extracted from the width and the
                    //worldX from the height.
                    float normalWidth  = width  * 1.0f / (mapWidth  - 1);
                    float normalHeight = height * 1.0f / (mapHeight - 1);

                    float worldX = transform.position.x + normalHeight * terrainData.size.x;
                    float worldZ = transform.position.z + normalWidth  * terrainData.size.z;

                    float[] newAlphas = AlphamapLogic.GetAlphamapForPoint(new Vector2(worldX, worldZ));

                    for(int alphaIndex = 0; alphaIndex < terrainData.splatPrototypes.Length; alphaIndex++) {
                        alphaMaps[width, height, alphaIndex] = newAlphas[alphaIndex];
                    }
                }
            }

            terrainData.SetAlphamaps(0, 0, alphaMaps);

            Terrain.Flush();

            RefreshAlphamapCoroutine = null;
        }

        private IEnumerator RefreshHeightmap_Perform() {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            var terrainData = Terrain.terrainData;

            int mapWidth  = terrainData.heightmapWidth;
            int mapHeight = terrainData.heightmapHeight;

            float[,] heights = terrainData.GetHeights(0, 0, mapWidth, mapHeight);

            for(int height = 0; height < mapHeight; height++) {
                for(int width = 0; width < mapWidth; width++) {
                    float normalWidth  = width  * 1.0f / (mapWidth  - 1);
                    float normalHeight = height * 1.0f / (mapHeight - 1);

                    float worldX = transform.position.x + normalHeight * terrainData.size.x;
                    float worldZ = transform.position.z + normalWidth  * terrainData.size.z;

                    heights[width, height] = HeightLogic.GetHeightForPoint(new Vector2(worldX, worldZ));
                }
            }

            terrainData.SetHeightsDelayLOD(0, 0, heights);

            Terrain.Flush();

            RefreshHeightmapCoroutine = null;
        }

        private IEnumerator RefreshWater_Perform() {
            yield return new WaitForEndOfFrame();

            StandingWater.Clear();

            foreach(var cell in Cells) {
                if(cell.Terrain.IsWater()) {
                    WaterTriangulator.TriangulateWaterForCell(cell, transform, StandingWater);
                }
            }

            StandingWater.Apply();

            RefreshWaterCoroutine = null;
        }

        private IEnumerator RefreshFeatures_Perform() {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            HexFeatureManager.Clear();

            foreach(var cell in Cells) {
                HexFeatureManager.AddFeatureLocationsForCell(cell);
            }

            HexFeatureManager.Apply();

            RefreshFeatureCoroutine = null;
        }

        private IEnumerator RefreshRivers_Perform() {
            yield return new WaitForEndOfFrame();

            RiverTriangulator.TriangulateRivers();

            RefreshRiversCoroutine = null;
        }

        private TerrainData BuildTerrainData(float width, float height) {
            var newData = new TerrainData();

            newData.alphamapResolution  = RenderConfig.TerrainAlphamapResolution;
            newData.heightmapResolution = RenderConfig.TerrainHeightmapResolution;

            newData.size = new Vector3(width, RenderConfig.TerrainMaxY, height);

            newData.splatPrototypes = RenderConfig.MapTextures.Select(
                mapTexture => new SplatPrototype() {
                    texture = mapTexture
                }
            ).ToArray();

            return newData;
        }

        #endregion
        
    }

}
