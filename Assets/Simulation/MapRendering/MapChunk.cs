﻿using System;
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

        #region from IMapChunk

        public IEnumerable<IHexCell> Cells {
            get { return cells; }
        }
        private List<IHexCell> cells = new List<IHexCell>();

        #endregion

        private Terrain         Terrain;
        private TerrainCollider TerrainCollider;

        private Coroutine RefreshAlphamapCoroutine;
        private Coroutine RefreshHeightmapCoroutine;
        private Coroutine RefreshWaterCoroutine;
        private Coroutine RefreshFeaturesCoroutine;
        private Coroutine RefreshCultureCoroutine;

        private Dictionary<IHexCell, HexMesh[]> CultureMeshesForCell = new Dictionary<IHexCell, HexMesh[]>();

        [SerializeField] private HexMesh StandingWater;
        [SerializeField] private HexMesh CultureMeshPrefab;




        private ITerrainAlphamapLogic AlphamapLogic;
        private ITerrainHeightLogic   HeightLogic;
        private IMapRenderConfig      RenderConfig;
        private IWaterTriangulator    WaterTriangulator;
        private IHexFeatureManager    HexFeatureManager;
        private IRiverTriangulator    RiverTriangulator;
        private ICultureTriangulator  CultureTriangulator;
        private DiContainer           Container;

        #endregion

        #region instance methods

        [Inject]
        private void InjectDependencies(
            ITerrainAlphamapLogic alphamapLogic, ITerrainHeightLogic heightLogic,
            IMapRenderConfig renderConfig, IWaterTriangulator waterTriangulator,
            IHexFeatureManager hexFeatureManager, IRiverTriangulator riverTriangulator,
            ICultureTriangulator cultureTriangulator, DiContainer container
        ) {
            AlphamapLogic       = alphamapLogic;
            HeightLogic         = heightLogic;
            RenderConfig        = renderConfig;
            WaterTriangulator   = waterTriangulator;
            HexFeatureManager   = hexFeatureManager;
            RiverTriangulator   = riverTriangulator;
            CultureTriangulator = cultureTriangulator;
            Container           = container;
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

            TerrainCollider = terrainGameObject.GetComponent<TerrainCollider>();

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

        public void RefreshCulture() {
            if(RefreshCultureCoroutine == null) {
                RefreshCultureCoroutine = StartCoroutine(RefreshCulture_Perform());
            }
        }

        public void RefreshFeatures() {
            if(RefreshFeaturesCoroutine == null) {
                RefreshFeaturesCoroutine = StartCoroutine(RefreshFeatures_Perform());
            }
        }

        public void RefreshAll() {
            RefreshAlphamap();
            RefreshHeightmap();
            RefreshWater();
            RefreshCulture();
            RefreshFeatures();

            if(RefreshRiversCoroutine == null) {
                RefreshRiversCoroutine = StartCoroutine(RefreshRivers_Perform());
            }
        }

        public bool DoesCellOverlapChunk(IHexCell cell) {
            return RenderConfig.CornersXZ.Any(corner => IsInTerrainBounds2D(corner + cell.AbsolutePositionXZ));
        }

        public bool IsInTerrainBounds2D(Vector2 positionXZ) {
            float xMin = transform.position.x;
            float xMax = transform.position.x + Terrain.terrainData.size.x;
            float zMin = transform.position.z;
            float zMax = transform.position.z + Terrain.terrainData.size.z;

            return positionXZ.x >= xMin && positionXZ.x <= xMax
                && positionXZ.y >= zMin && positionXZ.y <= zMax;
        }

        public Vector3 GetNearestPointOnTerrain(Vector3 fromLocation) {
            return TerrainCollider.ClosestPoint(fromLocation);
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

            RefreshFeaturesCoroutine = null;
        }

        private IEnumerator RefreshRivers_Perform() {
            yield return new WaitForEndOfFrame();

            RiverTriangulator.TriangulateRivers();

            RefreshRiversCoroutine = null;
        }

        private IEnumerator RefreshCulture_Perform() {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            foreach(var cell in Cells) {
                HexMesh[] cultureMeshes;

                if(!CultureMeshesForCell.TryGetValue(cell, out cultureMeshes)) {
                    cultureMeshes = new HexMesh[6];

                    CultureMeshesForCell[cell] = cultureMeshes;
                }

                foreach(var direction in EnumUtil.GetValues<HexDirection>()) {
                    HexMesh cultureMesh = cultureMeshes[(int)direction];

                    if(cultureMesh == null) {
                        cultureMesh = Container.InstantiatePrefabForComponent<HexMesh>(CultureMeshPrefab);

                        cultureMesh.transform.SetParent(transform, false);
                        cultureMesh.transform.position = Vector3.zero;

                        cultureMeshes[(int)direction] = cultureMesh;
                    }

                    cultureMesh.Clear();

                    CultureTriangulator.TriangulateCultureInDirection(cell, direction, cultureMesh);

                    cultureMesh.Apply();
                }
            }

            RefreshCultureCoroutine = null;
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
