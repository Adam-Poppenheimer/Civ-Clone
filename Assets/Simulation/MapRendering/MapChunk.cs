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

        #region internal types

        public class Pool : MonoMemoryPool<MapChunk> {

            protected override void OnDespawned(MapChunk item) {
                item.Clear();

                base.OnDespawned(item);
            }

        }

        #endregion

        #region static fields and properties

        private static Coroutine RefreshRiversCoroutine;        
        private static Coroutine RefreshFarmlandCoroutine;

        #endregion

        #region instance fields and properties

        #region from IMapChunk

        public IEnumerable<IHexCell> Cells {
            get { return cells; }
        }
        private List<IHexCell> cells = new List<IHexCell>();

        public Terrain Terrain { get; private set; }

        #endregion

        private TerrainCollider TerrainCollider;

        private Coroutine RefreshAlphamapCoroutine;
        private Coroutine RefreshHeightmapCoroutine;
        private Coroutine RefreshWaterCoroutine;
        private Coroutine RefreshFeaturesCoroutine;
        private Coroutine RefreshCultureCoroutine;
        private Coroutine RefreshVisibilityCoroutine;
        private Coroutine RefreshRoadsCoroutine;
        private Coroutine RefreshMarshesCoroutine;
        private Coroutine RefreshOasesCoroutine;

        private IHexMesh StandingWater {
            get {
                if(_standingWater == null) {
                    _standingWater = HexMeshFactory.Create("Standing Water", RenderConfig.StandingWaterData);

                    _standingWater.transform.SetParent(transform, false);
                }
                return _standingWater;
            }
            set { _standingWater = value; }
        }
        private IHexMesh _standingWater;

        private IHexMesh Culture {
            get {
                if(_culture == null) {
                    _culture = HexMeshFactory.Create("Culture", RenderConfig.CultureData);

                    _culture.transform.SetParent(transform, false);
                }
                return _culture;
            }
            set { _culture = value; }
        }
        private IHexMesh _culture;

        private IHexMesh Roads {
            get {
                if(_roads == null) {
                    _roads = HexMeshFactory.Create("Roads", RenderConfig.RoadData);

                    _roads.transform.SetParent(transform, false);
                }
                return _roads;
            }
        }
        private IHexMesh _roads;

        private IHexMesh MarshWater {
            get {
                if(_marshWater == null) {
                    _marshWater = HexMeshFactory.Create("Marsh Water", RenderConfig.MarshWaterData);

                    _marshWater.transform.SetParent(transform, false);
                }
                return _marshWater;
            }
        }
        private IHexMesh _marshWater;

        private IHexMesh OasisWater {
            get {
                if(_oasisWater == null) {
                    _oasisWater = HexMeshFactory.Create("Oasis Water", RenderConfig.OasisWaterData);

                    _oasisWater.transform.SetParent(transform, false);
                }
                return _oasisWater;
            }
        }
        private IHexMesh _oasisWater;

        private IHexMesh OasisLand {
            get {
                if(_oasisLand == null) {
                    _oasisLand = HexMeshFactory.Create("Oasis Land", RenderConfig.OasisLandData);

                    _oasisLand.transform.SetParent(transform, false);
                }
                return _oasisLand;
            }
        }
        private IHexMesh _oasisLand;


        [SerializeField] private TerrainBaker TerrainBaker;




        private ITerrainAlphamapLogic AlphamapLogic;
        private ITerrainHeightLogic   HeightLogic;
        private IMapRenderConfig      RenderConfig;
        private IWaterTriangulator    WaterTriangulator;
        private IHexFeatureManager    HexFeatureManager;
        private IRiverTriangulator    RiverTriangulator;
        private ICultureTriangulator  CultureTriangulator;
        private IHexCellShaderData    ShaderData;
        private IHexMeshFactory       HexMeshFactory;
        private IFarmTriangulator     FarmTriangulator;
        private IRoadTriangulator     RoadTriangulator;
        private IMarshTriangulator    MarshTriangulator;
        private IOasisTriangulator    OasisTriangulator;

        #endregion

        #region instance methods

        [Inject]
        private void InjectDependencies(
            ITerrainAlphamapLogic alphamapLogic, ITerrainHeightLogic heightLogic,
            IMapRenderConfig renderConfig, IWaterTriangulator waterTriangulator,
            IHexFeatureManager hexFeatureManager, IRiverTriangulator riverTriangulator,
            ICultureTriangulator cultureTriangulator, IHexCellShaderData shaderData,
            DiContainer container, IHexMeshFactory hexMeshFactory,
            IFarmTriangulator farmTriangulator, IRoadTriangulator roadTriangulator,
            IMarshTriangulator marshTriangulator, IOasisTriangulator oasisTriangulator
        ) {
            AlphamapLogic       = alphamapLogic;
            HeightLogic         = heightLogic;
            RenderConfig        = renderConfig;
            WaterTriangulator   = waterTriangulator;
            HexFeatureManager   = hexFeatureManager;
            RiverTriangulator   = riverTriangulator;
            CultureTriangulator = cultureTriangulator;
            ShaderData          = shaderData;
            HexMeshFactory      = hexMeshFactory;
            FarmTriangulator    = farmTriangulator;
            RoadTriangulator    = roadTriangulator;
            MarshTriangulator   = marshTriangulator;
            OasisTriangulator   = oasisTriangulator;
        }

        #region from IMapChunk

        public void AttachCell(IHexCell cell) {
            cells.Add(cell);
        }

        public void InitializeTerrain(Vector3 position, float width, float height) {
            var terrainData = BuildTerrainData(width, height);

            if(Terrain == null) {
                var terrainGameObject = Terrain.CreateTerrainGameObject(terrainData);

                terrainGameObject.layer = LayerMask.NameToLayer("Terrain");

                terrainGameObject.transform.SetParent(transform, false);

                Terrain = terrainGameObject.GetComponent<Terrain>();

                TerrainCollider = terrainGameObject.GetComponent<TerrainCollider>();
            }else {
                Terrain        .terrainData = terrainData;
                TerrainCollider.terrainData = terrainData;
            }

            transform.position = position;

            TerrainBaker.Initialize();

            var instancedTerrainMaterial = new Material(RenderConfig.TerrainMaterialTemplate);
            var instancedWaterMaterial   = new Material(RenderConfig.StandingWaterData.RenderingData.Material);

            instancedTerrainMaterial.SetTexture("_BakeTexture", TerrainBaker.TerrainTexture);
            instancedWaterMaterial  .SetTexture("_BakeTexture", TerrainBaker.WaterTexture);

            Vector4 bakeTextureDimensions = new Vector4(
                transform.position.x - RenderConfig.OuterRadius * 1.5f,
                transform.position.z - RenderConfig.OuterRadius * 1.5f,
                RenderConfig.ChunkWidth  + RenderConfig.OuterRadius * 3f,
                RenderConfig.ChunkHeight + RenderConfig.OuterRadius * 3f
            );

            instancedTerrainMaterial.SetVector("_BakeTextureDimensions", bakeTextureDimensions);
            instancedWaterMaterial  .SetVector("_BakeTextureDimensions", bakeTextureDimensions);

            instancedWaterMaterial.EnableKeyword("USE_BAKE_TEXTURE");

            Terrain.castShadows         = RenderConfig.TerrainCastsShadows;
            Terrain.materialType        = Terrain.MaterialType.Custom;
            Terrain.materialTemplate    = instancedTerrainMaterial;
            Terrain.heightmapPixelError = RenderConfig.TerrainHeightmapPixelError;

            StandingWater.OverrideMaterial(instancedWaterMaterial);

            Terrain.Flush();
        }

        public void Refresh(TerrainRefreshType refreshTypes) {
            if(((refreshTypes & TerrainRefreshType.Alphamap) == TerrainRefreshType.Alphamap) && RefreshAlphamapCoroutine == null) {
                RefreshAlphamapCoroutine = StartCoroutine(RefreshAlphamap_Perform());
            }

            if(((refreshTypes & TerrainRefreshType.Heightmap) == TerrainRefreshType.Heightmap) && RefreshHeightmapCoroutine == null) {
                RefreshHeightmapCoroutine = StartCoroutine(RefreshHeightmap_Perform());
            }

            if(((refreshTypes & TerrainRefreshType.Water) == TerrainRefreshType.Water) && RefreshWaterCoroutine == null) {
                RefreshWaterCoroutine = StartCoroutine(RefreshWater_Perform());
            }

            if(((refreshTypes & TerrainRefreshType.Culture) == TerrainRefreshType.Culture) && RefreshCultureCoroutine == null) {
                RefreshCultureCoroutine = StartCoroutine(RefreshCulture_Perform());
            }

            if(((refreshTypes & TerrainRefreshType.Features) == TerrainRefreshType.Features) && RefreshFeaturesCoroutine == null) {
                RefreshFeaturesCoroutine = StartCoroutine(RefreshFeatures_Perform());
            }

            if(((refreshTypes & TerrainRefreshType.Visibility) == TerrainRefreshType.Visibility) && RefreshVisibilityCoroutine == null) {
                RefreshVisibilityCoroutine = StartCoroutine(RefreshVisibility_Perform());
            }

            if(((refreshTypes & TerrainRefreshType.Farmland) == TerrainRefreshType.Farmland) && RefreshFarmlandCoroutine == null) {
                RefreshFarmlandCoroutine = StartCoroutine(RefreshFarmland_Perform());
            }

            if(((refreshTypes & TerrainRefreshType.Roads) == TerrainRefreshType.Roads) && RefreshRoadsCoroutine == null) {
                RefreshRoadsCoroutine = StartCoroutine(RefreshRoads_Perform());
            }

            if(((refreshTypes & TerrainRefreshType.Rivers) == TerrainRefreshType.Rivers) && RefreshRiversCoroutine == null) {
                RefreshRiversCoroutine = StartCoroutine(RefreshRivers_Perform());
            }

            if(((refreshTypes & TerrainRefreshType.Marshes) == TerrainRefreshType.Marshes) && RefreshMarshesCoroutine == null) {
                RefreshMarshesCoroutine = StartCoroutine(RefreshMarshes_Perform());
            }

            if(((refreshTypes & TerrainRefreshType.Oases) == TerrainRefreshType.Oases) && RefreshOasesCoroutine == null) {
                RefreshOasesCoroutine = StartCoroutine(RefreshOases_Perform());
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

        public void Clear() {
            StandingWater.Clear();
            Culture      .Clear();
            StopAllCoroutines();

            StopAllCoroutines();

            RefreshAlphamapCoroutine   = null;
            RefreshHeightmapCoroutine  = null;
            RefreshWaterCoroutine      = null;
            RefreshFeaturesCoroutine   = null;
            RefreshCultureCoroutine    = null;
            RefreshVisibilityCoroutine = null;
            RefreshFarmlandCoroutine   = null;
            RefreshRoadsCoroutine      = null;

            cells.Clear();

            HexMeshFactory.Destroy(StandingWater);
            HexMeshFactory.Destroy(Culture);

            StandingWater = null;
            Culture       = null;
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

            terrainData.SetHeights(0, 0, heights);

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

            Culture.Clear();

            foreach(var cell in Cells) {
                foreach(var direction in EnumUtil.GetValues<HexDirection>()) {
                    CultureTriangulator.TriangulateCultureInDirection(cell, direction, Culture);
                }
            }

            Culture.Apply();

            TerrainBaker.Bake();

            RefreshCultureCoroutine = null;
        }

        private IEnumerator RefreshVisibility_Perform() {
            yield return new WaitForEndOfFrame();

            while(RefreshRiversCoroutine != null) {
                yield return new WaitForEndOfFrame();
            }

            foreach(var cell in cells) {
                ShaderData.RefreshVisibility(cell);
            }

            RefreshVisibilityCoroutine = null;
        }

        private IEnumerator RefreshFarmland_Perform() {
            yield return new WaitForEndOfFrame();

            while(RefreshRiversCoroutine != null) {
                yield return new WaitForEndOfFrame();
            }

            FarmTriangulator.TriangulateFarmland();

            RefreshFarmlandCoroutine = null;
        }

        private IEnumerator RefreshRoads_Perform() {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            Roads.Clear();

            foreach(var cell in Cells) {
                RoadTriangulator.TriangulateRoads(cell, Roads);
            }

            Roads.Apply();

            TerrainBaker.Bake();

            RefreshRoadsCoroutine = null;
        }

        private IEnumerator RefreshMarshes_Perform() {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            MarshWater.Clear();
            

            foreach(var cell in Cells) {
                MarshTriangulator.TriangulateMarshes(cell, MarshWater);
                OasisTriangulator.TrianglateOasis   (cell, OasisWater, null);
            }

            MarshWater.Apply();

            TerrainBaker.Bake();

            RefreshMarshesCoroutine = null;
        }

        private IEnumerator RefreshOases_Perform() {
            yield return new WaitForEndOfFrame();

            OasisWater.Clear();
            OasisLand .Clear();

            foreach(var cell in Cells) {
                OasisTriangulator.TrianglateOasis(cell, OasisWater, OasisLand);
            }

            OasisWater.Apply();
            OasisLand .Apply();

            TerrainBaker.Bake();

            RefreshOasesCoroutine = null;
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
