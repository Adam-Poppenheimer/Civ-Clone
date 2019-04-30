using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.Profiling;

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

        #region instance fields and properties

        #region from IMapChunk

        public IEnumerable<IHexCell> Cells {
            get { return cells; }
        }
        private List<IHexCell> cells = new List<IHexCell>();

        public Terrain Terrain { get; private set; }

        public float Width  { get; private set; }
        public float Height { get; private set; }

        public bool IsRefreshing {
            get {
                return RefreshAlphamapCoroutine    != null ||
                       RefreshHeightmapCoroutine   != null || 
                       RefreshWaterCoroutine       != null || 
                       RefreshFeaturesCoroutine    != null || 
                       RefreshCultureCoroutine     != null || 
                       RefreshVisibilityCoroutine  != null || 
                       RefreshRoadsCoroutine       != null || 
                       RefreshMarshesCoroutine     != null || 
                       RefreshOasesCoroutine       != null || 
                       RefreshOrientationCoroutine != null;
                }
        }

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
        private Coroutine RefreshOrientationCoroutine;

        private IHexMesh OrientationMesh {
            get {
                if(_orientationMesh == null) {
                    _orientationMesh = HexMeshFactory.Create("Orientation Mesh", RenderConfig.OrientationMeshData);

                    _orientationMesh.transform.SetParent(transform, false);
                }

                return _orientationMesh;
            }
        }
        private IHexMesh _orientationMesh;

        private IHexMesh WeightsMesh {
            get {
                if(_weightsMesh == null) {
                    _weightsMesh = HexMeshFactory.Create("Weights Mesh", RenderConfig.WeightsMeshData);

                    _weightsMesh.transform.SetParent(transform, false);
                }

                return _weightsMesh;
            }
        }
        private IHexMesh _weightsMesh;

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

        public Texture2D LandBakeTexture {
            get { return landBakeTexture; }
        }
        [SerializeField] private Texture2D landBakeTexture;

        public Texture2D WaterBakeTexture {
            get { return waterBakeTexture; }
        }
        [SerializeField] private Texture2D waterBakeTexture;

        [SerializeField] private Texture2D OrientationTexture;
        [SerializeField] private Texture2D WeightsTexture;




        
        private ITerrainAlphamapLogic    AlphamapLogic;
        private ITerrainHeightLogic      HeightLogic;
        private IMapRenderConfig         RenderConfig;
        private IWaterTriangulator       WaterTriangulator;
        private IHexFeatureManager       HexFeatureManager;
        private ICultureTriangulator     CultureTriangulator;
        private IHexCellShaderData       ShaderData;
        private IHexMeshFactory          HexMeshFactory;
        private IRoadTriangulator        RoadTriangulator;
        private IMarshTriangulator       MarshTriangulator;
        private IOasisTriangulator       OasisTriangulator;
        private IOrientationTriangulator OrientationTriangulator;
        private IWeightsTriangulator     WeightsTriangulator;
        private ITerrainBaker            TerrainBaker;
        private IOrientationBaker        OrientationBaker;
        private IPointOrientationLogic   PointOrientationLogic;
        private IFullMapRefresher        FullMapRefresher;

        #endregion

        #region instance methods

        [Inject]
        private void InjectDependencies(
            ITerrainAlphamapLogic alphamapLogic, ITerrainHeightLogic heightLogic,
            IMapRenderConfig renderConfig, IWaterTriangulator waterTriangulator,
            IHexFeatureManager hexFeatureManager, ICultureTriangulator cultureTriangulator, IHexCellShaderData shaderData,
            DiContainer container, IHexMeshFactory hexMeshFactory, IRoadTriangulator roadTriangulator,
            IMarshTriangulator marshTriangulator, IOasisTriangulator oasisTriangulator,
            IOrientationTriangulator orientationTriangulator, IWeightsTriangulator weightsTriangulator,
            ITerrainBaker terrainBaker, IOrientationBaker orientationBaker,
            IPointOrientationLogic pointOrientationLogic, IFullMapRefresher fullMapRefresher
        ) {
            AlphamapLogic           = alphamapLogic;
            HeightLogic             = heightLogic;
            RenderConfig            = renderConfig;
            WaterTriangulator       = waterTriangulator;
            HexFeatureManager       = hexFeatureManager;
            CultureTriangulator     = cultureTriangulator;
            ShaderData              = shaderData;
            HexMeshFactory          = hexMeshFactory;
            RoadTriangulator        = roadTriangulator;
            MarshTriangulator       = marshTriangulator;
            OasisTriangulator       = oasisTriangulator;
            OrientationTriangulator = orientationTriangulator;
            WeightsTriangulator     = weightsTriangulator;
            TerrainBaker            = terrainBaker;
            OrientationBaker        = orientationBaker;
            PointOrientationLogic   = pointOrientationLogic;
            FullMapRefresher        = fullMapRefresher;
        }

        #region from IMapChunk

        public void AttachCell(IHexCell cell) {
            cells.Add(cell);

            Refresh(TerrainRefreshType.Orientation);
        }

        public void Initialize(Vector3 position, float width, float height) {
            Profiler.BeginSample("MapChunk.Initialize()");

            Width  = width;
            Height = height;

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

            var bakeData = RenderConfig.TerrainBakeTextureData;

            landBakeTexture = new Texture2D(
                Mathf.RoundToInt(bakeData.TexelsPerUnit * RenderConfig.ChunkWidth),
                Mathf.RoundToInt(bakeData.TexelsPerUnit * RenderConfig.ChunkHeight),
                TextureFormat.ARGB32, false
            );

            LandBakeTexture.filterMode = FilterMode.Point;
            LandBakeTexture.wrapMode   = TextureWrapMode.Clamp;
            LandBakeTexture.anisoLevel = 0;

            waterBakeTexture = new Texture2D(
                Mathf.RoundToInt(bakeData.TexelsPerUnit * RenderConfig.ChunkWidth),
                Mathf.RoundToInt(bakeData.TexelsPerUnit * RenderConfig.ChunkHeight),
                TextureFormat.ARGB32, false
            );

            waterBakeTexture.filterMode = FilterMode.Point;
            waterBakeTexture.wrapMode   = TextureWrapMode.Clamp;
            waterBakeTexture.anisoLevel = 0;

            OrientationTexture = new Texture2D(
                Mathf.RoundToInt(RenderConfig.OrientationTextureData.TexelsPerUnit * RenderConfig.ChunkWidth),
                Mathf.RoundToInt(RenderConfig.OrientationTextureData.TexelsPerUnit * RenderConfig.ChunkHeight),
                TextureFormat.ARGB32, false
            );

            OrientationTexture.filterMode = FilterMode.Point;
            OrientationTexture.wrapMode   = TextureWrapMode.Clamp;
            OrientationTexture.anisoLevel = 0;

            WeightsTexture = new Texture2D(
                Mathf.RoundToInt(RenderConfig.OrientationTextureData.TexelsPerUnit * RenderConfig.ChunkWidth),
                Mathf.RoundToInt(RenderConfig.OrientationTextureData.TexelsPerUnit * RenderConfig.ChunkHeight),
                TextureFormat.ARGB32, false
            );

            WeightsTexture.filterMode = FilterMode.Point;
            WeightsTexture.wrapMode   = TextureWrapMode.Clamp;
            WeightsTexture.anisoLevel = 0;

            transform.position = position;

            var instancedTerrainMaterial = new Material(RenderConfig.TerrainMaterialTemplate);
            var instancedWaterMaterial   = new Material(RenderConfig.StandingWaterData.RenderingData.Material);

            instancedTerrainMaterial.SetTexture("_BakeTexture", LandBakeTexture);
            instancedWaterMaterial  .SetTexture("_BakeTexture", waterBakeTexture);

            Vector4 bakeTextureDimensions = new Vector4(
                transform.position.x - RenderConfig.OuterRadius * 1.5f,
                transform.position.z - RenderConfig.OuterRadius * 1.5f,
                RenderConfig.ChunkWidth  + RenderConfig.OuterRadius * 3f,
                RenderConfig.ChunkHeight + RenderConfig.OuterRadius * 3f
            );

            instancedTerrainMaterial.SetVector("_BakeTextureDimensions", bakeTextureDimensions);
            instancedWaterMaterial  .SetVector("_BakeTextureDimensions", bakeTextureDimensions);

            instancedWaterMaterial.EnableKeyword("USE_BAKE_TEXTURE");

            Terrain.castShadows          = RenderConfig.TerrainCastsShadows;
            Terrain.basemapDistance      = RenderConfig.TerrainBasemapDistance;
            Terrain.materialType         = Terrain.MaterialType.Custom;
            Terrain.materialTemplate     = instancedTerrainMaterial;
            Terrain.heightmapPixelError  = RenderConfig.TerrainHeightmapPixelError;
            Terrain.drawTreesAndFoliage  = false;
            Terrain.editorRenderFlags    = TerrainRenderFlags.heightmap;
            Terrain.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

            StandingWater.OverrideMaterial(instancedWaterMaterial);

            Terrain.Flush();

            Profiler.EndSample();
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

            if(((refreshTypes & TerrainRefreshType.Farmland) == TerrainRefreshType.Farmland)) {
                FullMapRefresher.RefreshFarmland();
            }

            if(((refreshTypes & TerrainRefreshType.Roads) == TerrainRefreshType.Roads) && RefreshRoadsCoroutine == null) {
                RefreshRoadsCoroutine = StartCoroutine(RefreshRoads_Perform());
            }

            if(((refreshTypes & TerrainRefreshType.Rivers) == TerrainRefreshType.Rivers)) {
                FullMapRefresher.RefreshRivers();
            }

            if(((refreshTypes & TerrainRefreshType.Marshes) == TerrainRefreshType.Marshes) && RefreshMarshesCoroutine == null) {
                RefreshMarshesCoroutine = StartCoroutine(RefreshMarshes_Perform());
            }

            if(((refreshTypes & TerrainRefreshType.Oases) == TerrainRefreshType.Oases) && RefreshOasesCoroutine == null) {
                RefreshOasesCoroutine = StartCoroutine(RefreshOases_Perform());
            }

            if(((refreshTypes & TerrainRefreshType.Orientation) == TerrainRefreshType.Orientation) && RefreshOrientationCoroutine == null) {
                RefreshOrientationCoroutine = StartCoroutine(RefreshOrientation_Perform());
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
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            var terrainData = Terrain.terrainData;

            int mapWidth  = terrainData.alphamapWidth;
            int mapHeight = terrainData.alphamapHeight;

            float[,,] alphaMaps = terrainData.GetAlphamaps(0, 0, mapWidth, mapHeight);

            float maxTextureNormalX = Width  / RenderConfig.ChunkWidth;
            float maxTextureNormalZ = Height / RenderConfig.ChunkHeight;

            float terrainNormalX, terrainNormalZ, textureNormalX, textureNormalZ;

            PointOrientationData orientation;

            float[] newAlphas;

            int alphamapLength = terrainData.splatPrototypes.Length;

            int texelX, texelY;
            Color32 orientationColor;
            Color weightsColor;

            for(int height = 0; height < mapHeight; height++) {
                for(int width = 0; width < mapWidth; width++) {
                    //For some reason, terrainData seems to index its points
                    //as (y, x) rather than the more traditional (x, y), so
                    //we need to sample our texture accordingly
                    terrainNormalX = height * 1.0f / (mapHeight - 1);
                    terrainNormalZ = width  * 1.0f / (mapWidth  - 1);

                    textureNormalX = Mathf.Lerp(0f, maxTextureNormalX, terrainNormalX);
                    textureNormalZ = Mathf.Lerp(0f, maxTextureNormalZ, terrainNormalZ);

                    texelX = Mathf.RoundToInt(OrientationTexture.width  * textureNormalX);
                    texelY = Mathf.RoundToInt(OrientationTexture.height * textureNormalZ);

                    orientationColor = OrientationTexture.GetPixel(texelX, texelY);
                    weightsColor     = WeightsTexture    .GetPixel(texelX, texelY);

                    orientation = PointOrientationLogic.GetOrientationDataFromColors(orientationColor, weightsColor);

                    newAlphas = AlphamapLogic.GetAlphamapFromOrientation(orientation);

                    for(int alphaIndex = 0; alphaIndex < alphamapLength; alphaIndex++) {
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
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            var terrainData = Terrain.terrainData;

            Vector3 terrainSize = terrainData.size;

            int mapWidth  = terrainData.heightmapWidth;
            int mapHeight = terrainData.heightmapHeight;

            float[,] heights = terrainData.GetHeights(0, 0, mapWidth, mapHeight);

            float maxTextureNormalX = Width  / RenderConfig.ChunkWidth;
            float maxTextureNormalZ = Height / RenderConfig.ChunkHeight;

            float terrainNormalX, terrainNormalZ, textureNormalX, textureNormalZ, worldX, worldZ;

            int texelX, texelY;
            Color32 orientationColor;
            Color weightsColor;

            PointOrientationData orientation;

            float indexToNormalX = 1f / (mapHeight - 1f);
            float indexToNormalZ = 1f / (mapWidth  - 1f);

            

            for(int height = 0; height < mapHeight; height++) {
                for(int width = 0; width < mapWidth; width++) {
                    Profiler.BeginSample("Generic Math");

                    //For some reason, terrainData seems to index its points
                    //as (y, x) rather than the more traditional (x, y), so
                    //we need to sample our texture accordingly
                    terrainNormalX = height * indexToNormalX;
                    terrainNormalZ = width  * indexToNormalZ;

                    worldX = transform.position.x + terrainNormalX * terrainSize.x;
                    worldZ = transform.position.z + terrainNormalZ * terrainSize.z;

                    textureNormalX = Mathf.Lerp(0f, maxTextureNormalX, terrainNormalX);
                    textureNormalZ = Mathf.Lerp(0f, maxTextureNormalZ, terrainNormalZ);

                    texelX = Mathf.RoundToInt(OrientationTexture.width  * textureNormalX);
                    texelY = Mathf.RoundToInt(OrientationTexture.height * textureNormalZ);

                    Profiler.EndSample();

                    orientationColor = OrientationTexture.GetPixel(texelX, texelY);
                    weightsColor     = WeightsTexture    .GetPixel(texelX, texelY);

                    orientation = PointOrientationLogic.GetOrientationDataFromColors(orientationColor, weightsColor);

                    heights[width, height] = HeightLogic.GetHeightForPoint(new Vector2(worldX, worldZ), orientation);
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
            yield return new WaitForEndOfFrame();

            HexFeatureManager.Clear();

            foreach(var cell in Cells) {
                HexFeatureManager.AddFeatureLocationsForCell(cell);
            }

            HexFeatureManager.Apply();

            RefreshFeaturesCoroutine = null;
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

            TerrainBaker.BakeIntoTextures(LandBakeTexture, waterBakeTexture, this);

            RefreshCultureCoroutine = null;
        }

        private IEnumerator RefreshVisibility_Perform() {
            yield return new WaitForEndOfFrame();

            while(FullMapRefresher.IsRefreshingRivers) {
                yield return new WaitForEndOfFrame();
            }

            foreach(var cell in cells) {
                ShaderData.RefreshVisibility(cell);
            }

            RefreshVisibilityCoroutine = null;
        }

        private IEnumerator RefreshRoads_Perform() {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            Roads.Clear();

            foreach(var cell in Cells) {
                RoadTriangulator.TriangulateRoads(cell, Roads);
            }

            Roads.Apply();

            TerrainBaker.BakeIntoTextures(LandBakeTexture, waterBakeTexture, this);

            RefreshRoadsCoroutine = null;
        }

        private IEnumerator RefreshMarshes_Perform() {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            MarshWater.Clear();            

            foreach(var cell in Cells) {
                MarshTriangulator.TriangulateMarshes(cell, MarshWater);
            }

            MarshWater.Apply();

            TerrainBaker.BakeIntoTextures(LandBakeTexture, waterBakeTexture, this);

            RefreshMarshesCoroutine = null;
        }

        private IEnumerator RefreshOases_Perform() {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            OasisWater.Clear();
            OasisLand .Clear();

            foreach(var cell in Cells) {
                OasisTriangulator.TrianglateOasis(cell, OasisWater, OasisLand);
            }

            OasisWater.Apply();
            OasisLand .Apply();

            TerrainBaker.BakeIntoTextures(LandBakeTexture, waterBakeTexture, this);

            RefreshOasesCoroutine = null;
        }

        private IEnumerator RefreshOrientation_Perform() {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            OrientationMesh.Clear();
            WeightsMesh    .Clear();

            foreach(var cell in Cells) {
                OrientationTriangulator.TriangulateOrientation(cell, OrientationMesh);
                WeightsTriangulator    .TriangulateCellWeights(cell, WeightsMesh    );
            }

            OrientationMesh.Apply();
            WeightsMesh    .Apply();

            OrientationBaker.RenderOrientationFromMesh(OrientationTexture, WeightsTexture, transform);

            RefreshOrientationCoroutine = null;
        }

        private TerrainData BuildTerrainData(float width, float height) {
            var newData = new TerrainData();

            newData.alphamapResolution  = RenderConfig.TerrainAlphamapResolution;
            newData.heightmapResolution = RenderConfig.TerrainHeightmapResolution;
            newData.baseMapResolution   = RenderConfig.TerrainBasemapResolution;
            newData.SetDetailResolution(0, 8);

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
