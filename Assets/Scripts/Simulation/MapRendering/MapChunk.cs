using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Profiling;
using Unity.Collections;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Util;

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

        public Texture2D LandBakeTexture {
            get { return _landBakeTexture;  }
            set {
                _landBakeTexture = value;

                InstancedTerrainMaterial.SetTexture("_BakeTexture", _landBakeTexture);
            }
        }
        [SerializeField] private Texture2D _landBakeTexture;

        public Texture2D WaterBakeTexture {
            get { return _waterBakeTexture;  }
            set {
                _waterBakeTexture = value;

                InstancedWaterMaterial.SetTexture("_BakeTexture", _waterBakeTexture);
            }
        }
        [SerializeField] private Texture2D _waterBakeTexture;

        #endregion

        private TerrainCollider TerrainCollider;

        private TerrainRefreshType RefreshFlags = TerrainRefreshType.None;

        private Coroutine RefreshCoroutine;

        private IHexMesh StandingWater {
            get {
                if(standingWater == null) {
                    standingWater = HexMeshFactory.Create("Standing Water", RenderConfig.StandingWaterData);

                    standingWater.transform.SetParent(transform, false);
                }
                return standingWater;
            }
        }
        private IHexMesh standingWater;

        private IHexMesh Culture {
            get {
                if(culture == null) {
                    culture = HexMeshFactory.Create("Culture", RenderConfig.CultureData);

                    culture.transform.SetParent(transform, false);
                }
                return culture;
            }
        }
        private IHexMesh culture;

        private IHexMesh Roads {
            get {
                if(roads == null) {
                    roads = HexMeshFactory.Create("Roads", RenderConfig.RoadData);

                    roads.transform.SetParent(transform, false);
                }
                return roads;
            }
        }
        private IHexMesh roads;

        private IHexMesh MarshWater {
            get {
                if(marshWater == null) {
                    marshWater = HexMeshFactory.Create("Marsh Water", RenderConfig.MarshWaterData);

                    marshWater.transform.SetParent(transform, false);
                }
                return marshWater;
            }
        }
        private IHexMesh marshWater;

        private IHexMesh OasisWater {
            get {
                if(oasisWater == null) {
                    oasisWater = HexMeshFactory.Create("Oasis Water", RenderConfig.OasisWaterData);

                    oasisWater.transform.SetParent(transform, false);
                }
                return oasisWater;
            }
        }
        private IHexMesh oasisWater;

        private IHexMesh OasisLand {
            get {
                if(oasisLand == null) {
                    oasisLand = HexMeshFactory.Create("Oasis Land", RenderConfig.OasisLandData);

                    oasisLand.transform.SetParent(transform, false);
                }
                return oasisLand;
            }
        }
        private IHexMesh oasisLand;

        private Material InstancedTerrainMaterial;
        private Material InstancedWaterMaterial;




        
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

            //Refresh(TerrainRefreshType.Orientation);
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

            transform.position = position;

            InstancedTerrainMaterial = new Material(RenderConfig.TerrainMaterialTemplate);
            InstancedWaterMaterial   = new Material(RenderConfig.StandingWaterData.RenderingData.Material);

            Vector4 bakeTextureDimensions = new Vector4(
                transform.position.x - RenderConfig.OuterRadius * 1.5f,
                transform.position.z - RenderConfig.OuterRadius * 1.5f,
                RenderConfig.ChunkWidth  + RenderConfig.OuterRadius * 3f,
                RenderConfig.ChunkHeight + RenderConfig.OuterRadius * 3f
            );

            InstancedTerrainMaterial.SetVector("_BakeTextureDimensions", bakeTextureDimensions);
            InstancedWaterMaterial  .SetVector("_BakeTextureDimensions", bakeTextureDimensions);

            InstancedWaterMaterial.EnableKeyword("USE_BAKE_TEXTURE");

            Terrain.shadowCastingMode    = RenderConfig.TerrainShadowCastingMode;
            Terrain.basemapDistance      = RenderConfig.TerrainBasemapDistance;
            Terrain.materialType         = Terrain.MaterialType.Custom;
            Terrain.materialTemplate     = InstancedTerrainMaterial;
            Terrain.heightmapPixelError  = RenderConfig.TerrainHeightmapPixelError;
            Terrain.drawTreesAndFoliage  = false;
            Terrain.editorRenderFlags    = TerrainRenderFlags.Heightmap;
            Terrain.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

            StandingWater.OverrideMaterial(InstancedWaterMaterial);

            Terrain.Flush();
        }

        public void Refresh(TerrainRefreshType refreshTypes) {
            Profiler.BeginSample("MapChunk.Refresh()");

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

            Profiler.EndSample();
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
            ClearHexMeshes();

            StopAllCoroutines();

            RefreshCoroutine = null;

            centeredCells   .Clear();
            overlappingCells.Clear();
            

            if(LandBakeTexture != null) {
                Destroy(LandBakeTexture);
            }

            if(WaterBakeTexture != null) {
                Destroy(WaterBakeTexture);
            }

            if(InstancedTerrainMaterial != null) {
                Destroy(InstancedTerrainMaterial);
            }
            
            if(InstancedWaterMaterial != null) {
                Destroy(InstancedWaterMaterial);
            }
        }

        private void ClearHexMeshes() {
            if(standingWater != null) {
                standingWater.Clear();
                HexMeshFactory.Destroy(standingWater);

                standingWater = null;
            }

            if(culture != null) {
                culture.Clear();
                HexMeshFactory.Destroy(culture);

                culture = null;
            }

            if(roads != null) {
                roads.Clear();
                HexMeshFactory.Destroy(roads);

                roads = null;
            }

            if(marshWater != null) {
                marshWater.Clear();
                HexMeshFactory.Destroy(marshWater);

                marshWater = null;
            }

            if(oasisWater != null) {
                oasisWater.Clear();
                HexMeshFactory.Destroy(oasisWater);

                oasisWater = null;
            }

            if(oasisLand != null) {
                oasisLand.Clear();
                HexMeshFactory.Destroy(oasisLand);

                oasisLand = null;
            }
        }

        #endregion

        #region Unity messages

        private void OnDestroy() {
            Clear();
        }

        #endregion

        private IEnumerator Refresh_Perform() {
            yield return SkipFrame;

            while(FullMapRefresher.IsRefreshingRivers) {
                yield return SkipFrame;
            }

            yield return SkipFrame;

            bool flushTerrain = false;

            if((RefreshFlags & TerrainRefreshType.RequiresOrientation) != 0) {
                ChunkOrientationData orientationData = OrientationBaker.MakeOrientationRequestForChunk(this);

                while(!orientationData.IsReady) {
                    yield return SkipFrame;
                }

                if(((RefreshFlags & TerrainRefreshType.Alphamap) == TerrainRefreshType.Alphamap)) {
                    yield return StartCoroutine(RefreshAlphamap(orientationData));
                    flushTerrain = true;
                }

                if(((RefreshFlags & TerrainRefreshType.Heightmap) == TerrainRefreshType.Heightmap)) {
                    yield return StartCoroutine(RefreshHeightmap(orientationData));
                    flushTerrain = true;
                }

                OrientationBaker.ReleaseOrientationData(orientationData);
            }

            if(((RefreshFlags & TerrainRefreshType.Water) == TerrainRefreshType.Water)) {
                RefreshWater();
            }

            if(flushTerrain) {
                Terrain.Flush();
                yield return SkipFrame;
            }

            Profiler.BeginSample("Mono-threaded refresh");

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

            Profiler.EndSample();

            RefreshFlags = TerrainRefreshType.None;

            RefreshCoroutine = null;
        }

        private IEnumerator RefreshAlphamap(ChunkOrientationData orientationData) {
            var terrainData = Terrain.terrainData;

            var unsafeOrientationTexture = new AsyncTextureUnsafe<Color32>(orientationData.OrientationTexture);
            var unsafeWeightsTexture     = new AsyncTextureUnsafe<Color32>(orientationData.WeightsTexture);
            var unsafeDuckTexture        = new AsyncTextureUnsafe<Color32>(orientationData.DuckTexture);

            Vector3 terrainSize = terrainData.size;

            int mapWidth  = terrainData.alphamapWidth;
            int mapHeight = terrainData.alphamapHeight;

            int alphamapLength = RenderConfig.MapTextures.Count();

            float[,,] alphaMaps = terrainData.GetAlphamaps(0, 0, mapWidth, mapHeight);

            float maxTextureNormalX = Width  / RenderConfig.ChunkWidth;
            float maxTextureNormalZ = Height / RenderConfig.ChunkHeight;

            float indexToNormalX = 1f / (mapHeight - 1f);
            float indexToNormalZ = 1f / (mapWidth  - 1f);

            Vector3 chunkPosition = transform.position;

            var columnTasks = new Task[mapHeight];

            for(int height = 0; height < mapHeight; height++) {
                int cachedHeight = height;

                var indexBytes = new byte[2];

                PointOrientationData pointOrientation = new PointOrientationData();

                float[] returnMap       = new float[RenderConfig.MapTextures.Count()];
                float[] intermediateMap = new float[RenderConfig.MapTextures.Count()];

                var columnTask = new Task(() => {
                    for(int width = 0; width < mapWidth; width++) {
                        float terrainNormalX, terrainNormalZ, textureNormalX, textureNormalZ, worldX, worldZ;

                        Color32 orientationColor;
                        Color weightsColor, duckColor;

                        //For some reason, terrainData seems to index its points
                        //as (y, x) rather than the more traditional (x, y), so
                        //we need to sample our texture accordingly
                        terrainNormalX = cachedHeight * indexToNormalX;
                        terrainNormalZ = width  * indexToNormalZ;

                        worldX = chunkPosition.x + terrainNormalX * terrainSize.x;
                        worldZ = chunkPosition.z + terrainNormalZ * terrainSize.z;

                        textureNormalX = maxTextureNormalX * terrainNormalX;
                        textureNormalZ = maxTextureNormalZ * terrainNormalZ;

                        orientationColor = ColorCorrection.ARBG_To_RGBA(RawTextureSampler.SamplePoint(textureNormalX, textureNormalZ, unsafeOrientationTexture));
                        weightsColor     = ColorCorrection.ARBG_To_RGBA(RawTextureSampler.SamplePoint(textureNormalX, textureNormalZ, unsafeWeightsTexture));
                        duckColor        = ColorCorrection.ARBG_To_RGBA(RawTextureSampler.SamplePoint(textureNormalX, textureNormalZ, unsafeDuckTexture));

                        PointOrientationLogic.GetOrientationDataFromColors(
                            pointOrientation, indexBytes, orientationColor, weightsColor, duckColor
                        );

                        AlphamapLogic.GetAlphamapFromOrientation(returnMap, intermediateMap, pointOrientation);

                        for(int alphaIndex = 0; alphaIndex < alphamapLength; alphaIndex++) {
                            alphaMaps[width, cachedHeight, alphaIndex] = returnMap[alphaIndex];
                        }
                    }
                });

                columnTask.Start();

                columnTasks[cachedHeight] = columnTask;
            }

            while(columnTasks.Any(task => !task.IsCompleted)) {
                yield return SkipFrame;
            }

            terrainData.SetAlphamaps(0, 0, alphaMaps);
        }

        private IEnumerator RefreshHeightmap(ChunkOrientationData chunkOrientation) {
            var terrainData = Terrain.terrainData;

            var unsafeOrientationTexture = new AsyncTextureUnsafe<Color32>(chunkOrientation.OrientationTexture);
            var unsafeWeightsTexture     = new AsyncTextureUnsafe<Color32>(chunkOrientation.WeightsTexture);
            var unsafeDuckTexture        = new AsyncTextureUnsafe<Color32>(chunkOrientation.DuckTexture);

            var unsafeFlatlandsNoise = new AsyncTextureUnsafe<Color32>(RenderConfig.FlatlandsElevationNoiseSource);
            var unsafeHillsNoise     = new AsyncTextureUnsafe<Color32>(RenderConfig.HillsElevationNoiseSource);

            Vector3 terrainSize = terrainData.size;

            int mapWidth  = terrainData.heightmapWidth;
            int mapHeight = terrainData.heightmapHeight;

            float[,] heights = terrainData.GetHeights(0, 0, mapWidth, mapHeight);

            float maxTextureNormalX = Width  / RenderConfig.ChunkWidth;
            float maxTextureNormalZ = Height / RenderConfig.ChunkHeight;

            float indexToNormalX = 1f / (mapHeight - 1f);
            float indexToNormalZ = 1f / (mapWidth  - 1f);

            Vector3 chunkPosition = transform.position;

            var columnTasks = new Task[mapHeight];

            for(int height = 0; height < mapHeight; height++) {
                int cachedHeight = height;

                var indexBytes = new byte[2];

                PointOrientationData pointOrientation = new PointOrientationData();

                var columnTask = new Task(() => {
                    for(int width = 0; width < mapWidth; width++) {
                        float terrainNormalX, terrainNormalZ, textureNormalX, textureNormalZ, worldX, worldZ;

                        Color32 orientationColor;
                        Color weightsColor, duckColor;

                        //For some reason, terrainData seems to index its points
                        //as (y, x) rather than the more traditional (x, y), so
                        //we need to sample our texture accordingly
                        terrainNormalX = cachedHeight * indexToNormalX;
                        terrainNormalZ = width  * indexToNormalZ;

                        worldX = chunkPosition.x + terrainNormalX * terrainSize.x;
                        worldZ = chunkPosition.z + terrainNormalZ * terrainSize.z;

                        textureNormalX = maxTextureNormalX * terrainNormalX;
                        textureNormalZ = maxTextureNormalZ * terrainNormalZ;

                        orientationColor = ColorCorrection.ARBG_To_RGBA(RawTextureSampler.SamplePoint(textureNormalX, textureNormalZ, unsafeOrientationTexture));
                        weightsColor     = ColorCorrection.ARBG_To_RGBA(RawTextureSampler.SamplePoint(textureNormalX, textureNormalZ, unsafeWeightsTexture));
                        duckColor        = ColorCorrection.ARBG_To_RGBA(RawTextureSampler.SamplePoint(textureNormalX, textureNormalZ, unsafeDuckTexture));

                        PointOrientationLogic.GetOrientationDataFromColors(
                            pointOrientation, indexBytes, orientationColor, weightsColor, duckColor
                        );

                        heights[width, cachedHeight] = HeightLogic.GetHeightForPoint(
                            new Vector2(worldX, worldZ), pointOrientation, unsafeFlatlandsNoise, unsafeHillsNoise
                        );
                    }
                });

                columnTask.Start();

                columnTasks[cachedHeight] = columnTask;
            }

            while(columnTasks.Any(task => !task.IsCompleted)) {
                yield return SkipFrame;
            }

            terrainData.SetHeights(0, 0, heights);
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

            TerrainBaker.BakeIntoChunk(this);
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

            TerrainBaker.BakeIntoChunk(this);
        }

        private void RefreshMarshes() {
            MarshWater.Clear();            

            foreach(var cell in CenteredCells) {
                MarshTriangulator.TriangulateMarshes(cell, MarshWater);
            }

            MarshWater.Apply();

            TerrainBaker.BakeIntoChunk(this);
        }

        private void RefreshOases() {
            OasisWater.Clear();
            OasisLand .Clear();

            foreach(var cell in CenteredCells) {
                OasisTriangulator.TrianglateOasis(cell, OasisWater, OasisLand);
            }

            OasisWater.Apply();
            OasisLand .Apply();

            TerrainBaker.BakeIntoChunk(this);
        }

        private TerrainData BuildTerrainData(float width, float height) {
            var newData = new TerrainData();

            newData.alphamapResolution  = RenderConfig.TerrainAlphamapResolution;
            newData.heightmapResolution = RenderConfig.TerrainHeightmapResolution;
            newData.baseMapResolution   = RenderConfig.TerrainBasemapResolution;
            newData.SetDetailResolution(0, 8);

            newData.size = new Vector3(width, RenderConfig.TerrainMaxY, height);

            newData.terrainLayers = RenderConfig.MapTextures.Select(
                mapTexture => new TerrainLayer() {
                    name           = mapTexture.name,
                    diffuseTexture = mapTexture
                }
            ).ToArray();

            return newData;
        }

        #endregion
        
    }

}
