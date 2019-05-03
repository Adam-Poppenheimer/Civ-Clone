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

        #region static fields and properties

        private static WaitForEndOfFrame SkipFrame = new WaitForEndOfFrame();

        #endregion

        #region instance fields and properties

        #region from IMapChunk

        public IEnumerable<IHexCell> CenteredCells {
            get { return centeredCells; }
        }
        private List<IHexCell> centeredCells = new List<IHexCell>();

        public IEnumerable<IHexCell> OverlappingCells {
            get { return overlappingCells; }
        }
        private List<IHexCell> overlappingCells = new List<IHexCell>();

        public Terrain Terrain { get; private set; }

        public float Width  { get; private set; }
        public float Height { get; private set; }

        public bool IsRefreshing {
            get { return RefreshCoroutine != null; }
        }

        #endregion

        private TerrainCollider TerrainCollider;

        private TerrainRefreshType RefreshFlags = TerrainRefreshType.None;

        private Coroutine RefreshCoroutine;

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
            TerrainBaker            = terrainBaker;
            OrientationBaker        = orientationBaker;
            PointOrientationLogic   = pointOrientationLogic;
            FullMapRefresher        = fullMapRefresher;
        }

        #region from IMapChunk

        public void AttachCell(IHexCell cell, bool isCentered) {
            if(isCentered) {
                centeredCells.Add(cell);
            }else {
                overlappingCells.Add(cell);
            }            

            Refresh(TerrainRefreshType.Orientation);
        }

        public void Initialize(Vector3 position, float width, float height) {
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
        }

        public void Refresh(TerrainRefreshType refreshTypes) {
            if(RefreshCoroutine == null) {
                RefreshCoroutine = StartCoroutine(Refresh_Perform());
            }

            if(((refreshTypes & TerrainRefreshType.Rivers) == TerrainRefreshType.Rivers)) {
                FullMapRefresher.RefreshRivers();
            }

            if(((refreshTypes & TerrainRefreshType.Farmland) == TerrainRefreshType.Farmland)) {
                FullMapRefresher.RefreshFarmland();
            }

            RefreshFlags |= refreshTypes;
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

            RefreshCoroutine = null;

            centeredCells   .Clear();
            overlappingCells.Clear();

            HexMeshFactory.Destroy(StandingWater);
            HexMeshFactory.Destroy(Culture);

            StandingWater = null;
            Culture       = null;
        }

        #endregion

        private IEnumerator Refresh_Perform() {
            yield return SkipFrame;

            while(FullMapRefresher.IsRefreshingRivers) {
                yield return SkipFrame;
            }

            yield return SkipFrame;

            bool flushTerrain = false;

            if(((RefreshFlags & TerrainRefreshType.Orientation) == TerrainRefreshType.Orientation)) {
                RefreshOrientation();
            }

            if(((RefreshFlags & TerrainRefreshType.Alphamap) == TerrainRefreshType.Alphamap)) {
                RefreshAlphamap();
                flushTerrain = true;
            }

            if(((RefreshFlags & TerrainRefreshType.Heightmap) == TerrainRefreshType.Heightmap)) {
                RefreshHeightmap();
                flushTerrain = true;
            }

            if(((RefreshFlags & TerrainRefreshType.Water) == TerrainRefreshType.Water)) {
                RefreshWater();
            }

            if(flushTerrain) {
                Terrain.Flush();
                yield return SkipFrame;
            }

            if(((RefreshFlags & TerrainRefreshType.Culture) == TerrainRefreshType.Culture)) {
                RefreshCulture();
            }

            if(((RefreshFlags & TerrainRefreshType.Features) == TerrainRefreshType.Features)) {
                RefreshFeatures();
            }

            if(((RefreshFlags & TerrainRefreshType.Roads) == TerrainRefreshType.Roads)) {
                RefreshRoads();
            }

            if(((RefreshFlags & TerrainRefreshType.Marshes) == TerrainRefreshType.Marshes)) {
                RefreshMarshes();
            }

            if(((RefreshFlags & TerrainRefreshType.Oases) == TerrainRefreshType.Oases)) {
                RefreshOases();
            }

            if(((RefreshFlags & TerrainRefreshType.Visibility) == TerrainRefreshType.Visibility)) {
                RefreshVisibility();
            }

            RefreshFlags = TerrainRefreshType.None;

            RefreshCoroutine = null;
        }

        private void RefreshAlphamap() {
            Profiler.BeginSample("RefreshAlphamap()");

            var terrainData = Terrain.terrainData;

            var orientationTexture = OrientationBaker.OrientationTexture;
            var weightsTexture     = OrientationBaker.WeightsTexture;
            var duckTexture        = OrientationBaker.RiverDuckTexture;

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
            Color weightsColor, duckColor;

            for(int height = 0; height < mapHeight; height++) {
                for(int width = 0; width < mapWidth; width++) {
                    //For some reason, terrainData seems to index its points
                    //as (y, x) rather than the more traditional (x, y), so
                    //we need to sample our texture accordingly
                    terrainNormalX = height * 1.0f / (mapHeight - 1);
                    terrainNormalZ = width  * 1.0f / (mapWidth  - 1);

                    textureNormalX = Mathf.Lerp(0f, maxTextureNormalX, terrainNormalX);
                    textureNormalZ = Mathf.Lerp(0f, maxTextureNormalZ, terrainNormalZ);

                    texelX = Mathf.RoundToInt(orientationTexture.width  * textureNormalX);
                    texelY = Mathf.RoundToInt(orientationTexture.height * textureNormalZ);

                    orientationColor = orientationTexture.GetPixel(texelX, texelY);
                    weightsColor     = weightsTexture    .GetPixel(texelX, texelY);
                    duckColor        = duckTexture       .GetPixel(texelX, texelY);

                    orientation = PointOrientationLogic.GetOrientationDataFromColors(
                        orientationColor, weightsColor, duckColor
                    );

                    newAlphas = AlphamapLogic.GetAlphamapFromOrientation(orientation);

                    for(int alphaIndex = 0; alphaIndex < alphamapLength; alphaIndex++) {
                        alphaMaps[width, height, alphaIndex] = newAlphas[alphaIndex];
                    }
                }
            }

            terrainData.SetAlphamaps(0, 0, alphaMaps);

            Profiler.EndSample();
        }

        private void RefreshHeightmap() {
            Profiler.BeginSample("RefreshHeightmap()");

            var terrainData = Terrain.terrainData;

            var orientationTexture = OrientationBaker.OrientationTexture;
            var weightsTexture     = OrientationBaker.WeightsTexture;
            var duckTexture        = OrientationBaker.RiverDuckTexture;

            Vector3 terrainSize = terrainData.size;

            int mapWidth  = terrainData.heightmapWidth;
            int mapHeight = terrainData.heightmapHeight;

            float[,] heights = terrainData.GetHeights(0, 0, mapWidth, mapHeight);

            float maxTextureNormalX = Width  / RenderConfig.ChunkWidth;
            float maxTextureNormalZ = Height / RenderConfig.ChunkHeight;

            float terrainNormalX, terrainNormalZ, textureNormalX, textureNormalZ, worldX, worldZ;

            int texelX, texelY;
            Color32 orientationColor;
            Color weightsColor, duckColor;

            PointOrientationData orientation;

            float indexToNormalX = 1f / (mapHeight - 1f);
            float indexToNormalZ = 1f / (mapWidth  - 1f);

            for(int height = 0; height < mapHeight; height++) {
                for(int width = 0; width < mapWidth; width++) {
                    //For some reason, terrainData seems to index its points
                    //as (y, x) rather than the more traditional (x, y), so
                    //we need to sample our texture accordingly
                    terrainNormalX = height * indexToNormalX;
                    terrainNormalZ = width  * indexToNormalZ;

                    worldX = transform.position.x + terrainNormalX * terrainSize.x;
                    worldZ = transform.position.z + terrainNormalZ * terrainSize.z;

                    textureNormalX = maxTextureNormalX * terrainNormalX;
                    textureNormalZ = maxTextureNormalZ * terrainNormalZ;

                    texelX = Mathf.RoundToInt(orientationTexture.width  * textureNormalX);
                    texelY = Mathf.RoundToInt(orientationTexture.height * textureNormalZ);

                    orientationColor = orientationTexture.GetPixel(texelX, texelY);
                    weightsColor     = weightsTexture    .GetPixel(texelX, texelY);
                    duckColor        = duckTexture       .GetPixel(texelX, texelY);

                    orientation = PointOrientationLogic.GetOrientationDataFromColors(
                        orientationColor, weightsColor, duckColor
                    );

                    heights[width, height] = HeightLogic.GetHeightForPoint(new Vector2(worldX, worldZ), orientation);
                }
            }

            terrainData.SetHeights(0, 0, heights);

            Profiler.EndSample();
        }

        private void RefreshWater() {
            StandingWater.Clear();

            foreach(var cell in CenteredCells) {
                if(cell.Terrain.IsWater()) {
                    WaterTriangulator.TriangulateWaterForCell(cell, transform, StandingWater);
                }
            }

            StandingWater.Apply();
        }

        private void RefreshFeatures() {
            HexFeatureManager.Clear();

            foreach(var cell in CenteredCells) {
                HexFeatureManager.AddFeatureLocationsForCell(cell);
            }

            HexFeatureManager.Apply();
        }

        private void RefreshCulture() {
            Culture.Clear();

            foreach(var cell in CenteredCells) {
                foreach(var direction in EnumUtil.GetValues<HexDirection>()) {
                    CultureTriangulator.TriangulateCultureInDirection(cell, direction, Culture);
                }
            }

            Culture.Apply();

            TerrainBaker.BakeIntoTextures(LandBakeTexture, waterBakeTexture, this);
        }

        private void RefreshVisibility() {
            foreach(var cell in centeredCells) {
                ShaderData.RefreshVisibility(cell);
            }
        }

        private void RefreshRoads() {
            Roads.Clear();

            foreach(var cell in CenteredCells) {
                RoadTriangulator.TriangulateRoads(cell, Roads);
            }

            Roads.Apply();

            TerrainBaker.BakeIntoTextures(LandBakeTexture, waterBakeTexture, this);
        }

        private void RefreshMarshes() {
            MarshWater.Clear();            

            foreach(var cell in CenteredCells) {
                MarshTriangulator.TriangulateMarshes(cell, MarshWater);
            }

            MarshWater.Apply();

            TerrainBaker.BakeIntoTextures(LandBakeTexture, waterBakeTexture, this);
        }

        private void RefreshOases() {
            OasisWater.Clear();
            OasisLand .Clear();

            foreach(var cell in CenteredCells) {
                OasisTriangulator.TrianglateOasis(cell, OasisWater, OasisLand);
            }

            OasisWater.Apply();
            OasisLand .Apply();

            TerrainBaker.BakeIntoTextures(LandBakeTexture, waterBakeTexture, this);
        }

        private void RefreshOrientation() {
            OrientationBaker.RenderOrientationFromChunk(this);
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
