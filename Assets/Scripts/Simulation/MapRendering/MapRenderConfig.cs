using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.Rendering;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    [CreateAssetMenu(menuName = "Civ Clone/Map Rendering/Render Config")]
    public class MapRenderConfig : ScriptableObject, IMapRenderConfig {

        #region instance fields and properties

        #region from IMapRenderConfig

        public int RandomSeed {
            get { return _randomSeed; }
        }
        [SerializeField] private int _randomSeed = 0;

        public float NoiseScale {
            get { return _noiseScale; }
        }
        [SerializeField] private float _noiseScale = 1;

        public INoiseTexture GenericNoiseSource {
            get { return genericNoiseSource_Wrapped; }
        }
        private INoiseTexture genericNoiseSource_Wrapped;
        [SerializeField] private Texture2D genericNoiseSource = null;

        public Texture2D FlatlandsElevationNoiseSource {
            get { return _flatlandsElevationNoiseSource; }
        }
        [SerializeField] private Texture2D _flatlandsElevationNoiseSource = null;

        public Texture2D HillsElevationNoiseSource  {
            get { return _hillsElevationNoiseSource; }
        }
        [SerializeField] private Texture2D _hillsElevationNoiseSource = null;        

        public int NoiseHashGridSize {
            get { return _noiseHashGridSize; }
        }
        [SerializeField] private int _noiseHashGridSize = 10;

        public float NoiseHashGridScale {
            get { return _noiseHashGridScale; }
        }
        [SerializeField] private float _noiseHashGridScale = 1f;

        public float CellPerturbStrengthXZ {
            get { return _cellPerturbStrengthXZ; }
        }
        [SerializeField] private float _cellPerturbStrengthXZ = 0f;

        public float FlatlandsElevationNoiseStrength {
            get { return _flatlandsElevationNoiseStrength; }
        }
        [SerializeField, Range(0f, 1f)] private float _flatlandsElevationNoiseStrength = 0f;

        public float HillsElevationNoiseStrength {
            get { return _hillsElevationNoiseStrength; }
        }
        [SerializeField, Range(0f, 1f)] private float _hillsElevationNoiseStrength = 0f;



        public float OuterToInner {
            get { return 0.866025404f; }
        }

        public float InnerToOuter {
            get { return 1f / OuterToInner; }
        }

        public float OuterRadius {
            get { return _outerRadius; }
        }
        [SerializeField] private float _outerRadius = 10f;

        public float InnerRadius {
            get { return OuterRadius * OuterToInner; }
        }

        public float SolidFactor {
            get { return _solidFactor; }
        }
        [SerializeField] private float _solidFactor = 1f;

        public float BlendFactor {
            get { return 1f - SolidFactor; }
        }

        public IEnumerable<Vector3> Corners {
            get { return corners; }
        }
        private Vector3[] corners = null;

        public IEnumerable<Vector2> CornersXZ {
            get { return cornersXZ; }
        }
        private Vector2[] cornersXZ = null;

        

        public float ChunkWidth {
            get { return _chunkWidth; }
        }
        [SerializeField] private int _chunkWidth = 10;

        public float ChunkHeight {
            get { return _chunkHeight; }
        }
        [SerializeField] private float _chunkHeight = 10;

        public float TerrainMaxY {
            get { return _terrainMaxY; }
        }
        [SerializeField, Range(10, 100)] private float _terrainMaxY = 10f;

        public int TerrainAlphamapResolution  {
            get { return _terrainAlphamapResolution; }
        }
        [SerializeField] private int _terrainAlphamapResolution = 33;

        public int TerrainHeightmapResolution {
            get { return _terrainHeightmapResolution; }
        }
        [SerializeField] private int _terrainHeightmapResolution = 33;

        public int TerrainBasemapResolution {
            get { return _terrainBasemapResolution; }
        }
        [SerializeField] private int _terrainBasemapResolution = 16;

        public float TerrainBasemapDistance {
            get { return _terrainBasemapDistance; }
        }
        [SerializeField] private float _terrainBasemapDistance = 1000;

        public ShadowCastingMode TerrainShadowCastingMode {
            get { return _terrainShadowCastingMode; }
        }
        [SerializeField] private ShadowCastingMode _terrainShadowCastingMode = ShadowCastingMode.Off;

        public int TerrainHeightmapPixelError {
            get { return _terrainHeightmapPixelError; }
        }
        [SerializeField, Range(1, 1000)] private int _terrainHeightmapPixelError = 1;

        public LayerMask MapCollisionMask {
            get { return _mapCollisionMask; }
        }
        [SerializeField] private LayerMask _mapCollisionMask = 0;

        public Material TerrainMaterialTemplate {
            get { return _terrainMaterialTemplate; }
        }
        [SerializeField] private Material _terrainMaterialTemplate = null;

        public IEnumerable<Texture2D> MapTextures {
            get { return _mapTextures; }
        }
        [SerializeField] private List<Texture2D> _mapTextures = null;

        public int SeaFloorTextureIndex {
            get { return _seaFloorTextureIndex; }
        }
        [SerializeField] private int _seaFloorTextureIndex = 0;

        public int MountainTextureIndex {
            get { return _mountainTextureIndex; }
        }
        [SerializeField] private int _mountainTextureIndex = 0;

        public float[] RiverAlphamap {
            get { return _riverAlphamap; }
        }
        [SerializeField] private float[] _riverAlphamap = null;

        public float[] FloodPlainsAlphamap {
            get { return _floodPlainsAlphamap; }
        }
        [SerializeField] private float[] _floodPlainsAlphamap = null;



        public float WaterSurfaceElevation  {
            get { return _waterSurfaceElevation; }
        }
        [SerializeField, Range(0f, 1f)] private float _waterSurfaceElevation = 0f;

        public float SeaFloorElevation  {
            get { return _seaFloorElevation; }
        }
        [SerializeField, Range(0f, 1f)] private float _seaFloorElevation = 0f;

        public float FlatlandsBaseElevation  {
            get { return _flatlandsBaseElevation; }
        }
        [SerializeField, Range(0f, 1f)] private float _flatlandsBaseElevation = 0f;

        public float HillsBaseElevation  {
            get { return _hillsBaseElevation; }
        }
        [SerializeField, Range(0f, 1f)] private float _hillsBaseElevation = 0f;

        public float MountainPeakElevation  {
            get { return _mountainPeakElevation; }
        }
        [SerializeField, Range(0f, 1f)] private float _mountainPeakElevation = 0f;

        public float MountainRidgeElevation  {
            get { return _mountainRidgeElevation; }
        }
        [SerializeField, Range(0f, 1f)] private float _mountainRidgeElevation = 0f;

        public float RiverTroughElevation  {
            get { return _riverTroughElevation; }
        }
        [SerializeField, Range(0f, 1f)] private float _riverTroughElevation = 0f;


        public float WaterY {
            get { return WaterSurfaceElevation * TerrainMaxY; }
        }

        public Color ShallowWaterColor {
            get { return _shallowWaterColor; }
        }
        [SerializeField] private Color _shallowWaterColor = Color.clear;

        public Color DeepWaterColor {
            get { return _deepWaterColor; }
        }
        [SerializeField] private Color _deepWaterColor = Color.clear;

        public Color FreshWaterColor {
            get { return _freshWaterColor; }
        }
        [SerializeField] private Color _freshWaterColor = Color.clear;

        public Color RiverWaterColor {
            get { return _riverWaterColor; }
        }
        [SerializeField] private Color _riverWaterColor = Color.clear;


        public float RiverMaxSurfaceWidth {
            get { return _riverMaxInnerWidth; }
        }
        [SerializeField, Range(0f, 10f)] private float _riverMaxInnerWidth = 0f;

        public float RiverMaxBankWidth {
            get { return _riverMaxBankWidth; }
        }
        [SerializeField, Range(0f, 5f)] private float _riverMaxBankWidth = 0f;

        public float RiverDuckWidth {
            get { return _riverDuckWidth; }
        }
        [SerializeField, Range(0f, 5f)] private float _riverDuckWidth = 0f;

        public float RiverCurveStrength {
            get { return _riverCurveStrength; }
        }
        [SerializeField] private float _riverCurveStrength = 0f;

        public float RiverWideningRate {
            get { return _riverWideningRate; }
        }
        [SerializeField, Range(0f, 100f)] private float _riverWideningRate = 0f;

        public float RiverWidthNoise {
            get { return _riverWidthNoise; }
        }
        [SerializeField, Range(0f, 1f)] private float _riverWidthNoise = 0f;

        public int RiverCurvesForMaxWidth {
            get { return _riverCurvesForMaxWidth; }
        }
        [SerializeField, Range(0f, 30f)] private int _riverCurvesForMaxWidth = 10;

        public int RiverQuadsPerCurve {
            get { return _riverQuadsPerCurve; }
        }
        [SerializeField, Range(1f, 100f)] private int _riverQuadsPerCurve = 1;

        public float RiverFlowSpeed {
            get { return _riverFlowSpeed; }
        }
        [SerializeField] private float _riverFlowSpeed = 0f;




        public float CultureWidthPercent {
            get { return _cultureWidthPercent; }
        }
        [SerializeField, Range(0f, 1f)] private float _cultureWidthPercent = 0.1f;



        public HexMeshData StandingWaterData {
            get { return _standingWaterData; }
        }
        [SerializeField] private HexMeshData _standingWaterData = null;

        public HexMeshData RiverSurfaceData {
            get { return _riverSurfaceData; }
        }
        [SerializeField] private HexMeshData _riverSurfaceData = null;

        public HexMeshData RiverBankData {
            get { return _riverBankData; }
        }
        [SerializeField] private HexMeshData _riverBankData = null;

        public HexMeshData RiverDuckData {
            get { return _riverDuckData; }
        }
        [SerializeField] private HexMeshData _riverDuckData = null;

        public HexMeshData CultureData {
            get { return _cultureData; }
        }
        [SerializeField] private HexMeshData _cultureData = null;

        public HexMeshData FarmlandData {
            get { return _farmlandData; }
        }
        [SerializeField] private HexMeshData _farmlandData = null;

        public HexMeshData RoadData {
            get { return _roadData; }
        }
        [SerializeField] private HexMeshData _roadData = null;

        public HexMeshData MarshWaterData {
            get { return _marshWaterData; }
        }
        [SerializeField] private HexMeshData _marshWaterData = null;

        public HexMeshData OasisWaterData {
            get { return _oasisWaterData; }
        }
        [SerializeField] private HexMeshData _oasisWaterData = null;

        public HexMeshData OasisLandData {
            get { return _oasisLandData; }
        }
        [SerializeField] private HexMeshData _oasisLandData = null;

        public HexMeshData OrientationMeshData {
            get { return _orientationMeshData; }
        }
        [SerializeField] private HexMeshData _orientationMeshData = null;

        public HexMeshData WeightsMeshData {
            get { return _weightsMeshData; }
        }
        [SerializeField] private HexMeshData _weightsMeshData = null;



        public ReadOnlyCollection<Color> FarmColors {
            get { return _farmColors.AsReadOnly(); }
        }
        [SerializeField] private List<Color> _farmColors = null;

        public float FarmPatchMinWidth {
            get { return _farmPatchMinWidth; }
        }
        [SerializeField, Range(1f, 20f)] private float _farmPatchMinWidth = 1f;

        public float FarmPatchMaxWidth {
            get { return _farmPatchMaxWidth; }
        }
        [SerializeField, Range(1f, 20f)] private float _farmPatchMaxWidth = 1f;



        public int RoadQuadsPerCurve {
            get { return _roadQuadsPerCurve; }
        }
        [SerializeField] private int _roadQuadsPerCurve = 1;

        public float RoadWidth {
            get { return _roadWidth; }
        }
        [SerializeField] private float _roadWidth = 1f;

        public float RoadVRepeatLength {
            get { return _roadVRepeatLength; }
        }
        [SerializeField] private float _roadVRepeatLength = 1f;



        public int OasisBoundarySegments {
            get { return _oasisBoundarySegments; }
        }
        [SerializeField] private int _oasisBoundarySegments = 2;

        public float OasisWaterRadius {
            get { return _oasisWaterRadius; }
        }
        [SerializeField] private float _oasisWaterRadius = 1f;

        public float OasisLandWidth {
            get { return _oasisLandWidth; }
        }
        [SerializeField] private float _oasisLandWidth = 1f;




        public OrientationBakingDataData OrientationTextureData {
            get { return _orientationTextureData; }
        }
        [SerializeField] private OrientationBakingDataData _orientationTextureData = null;

        public int MaxOrientationTextures {
            get { return _maxOrientationTextures; }
        }
        [SerializeField] private int _maxOrientationTextures = 0;

        public Shader RiverWeightShader {
            get { return _riverWeightShader; }
        }
        [SerializeField] private Shader _riverWeightShader = null;        

        #endregion

        #endregion

        #region instance methods

        #region Unity messages

        private void OnEnable() {
            SetCorners();
            WrapTextures();
        }

        private void Awake() {
            SetCorners();
            WrapTextures();
        }

        private void OnValidate() {
            SetCorners();
            WrapTextures();
        }

        private void SetCorners() {
            corners = new Vector3[] {
                new Vector3(0f,           0f,  OuterRadius),
                new Vector3(InnerRadius,  0f,  0.5f * OuterRadius),
                new Vector3(InnerRadius,  0f, -0.5f * OuterRadius),
                new Vector3(0f,           0f, -OuterRadius),
                new Vector3(-InnerRadius, 0f, -0.5f * OuterRadius),
                new Vector3(-InnerRadius, 0f,  0.5f * OuterRadius),
                new Vector3(0f,           0f,  OuterRadius)
            };

            cornersXZ = new Vector2[] {
                new Vector2(0f,            OuterRadius),
                new Vector2(InnerRadius,   0.5f * OuterRadius),
                new Vector2(InnerRadius,  -0.5f * OuterRadius),
                new Vector2(0f,           -OuterRadius),
                new Vector2(-InnerRadius, -0.5f * OuterRadius),
                new Vector2(-InnerRadius,  0.5f * OuterRadius),
                new Vector2(0f,            OuterRadius),
            };
        }

        private void WrapTextures() {
            if(genericNoiseSource != null) {
                genericNoiseSource_Wrapped = new NoiseTexture(genericNoiseSource);
            }
        }

        #endregion

        #region from IHexMapRenderConfig

        public Vector3 GetFirstCorner(HexDirection direction) {
            return corners[(int)direction];
        }

        public Vector3 GetSecondCorner(HexDirection direction) {
            return corners[(int)direction + 1];
        }

        public Vector3 GetFirstSolidCorner(HexDirection direction) {
            return corners[(int)direction] * SolidFactor;
        }

        public Vector3 GetSecondSolidCorner(HexDirection direction) {
            return corners[(int)direction + 1] * SolidFactor;
        }

        public Vector3 GetEdgeMidpoint(HexDirection direction) {
            return (corners[(int)direction] + corners[(int)direction + 1]) * 0.5f;
        }

        public Vector3 GetSolidEdgeMidpoint(HexDirection direction) {
            return (corners[(int)direction] * SolidFactor + corners[(int)direction + 1] * SolidFactor) * 0.5f;
        }



        public Vector2 GetFirstCornerXZ(HexDirection direction) {
            return cornersXZ[(int)direction];
        }

        public Vector2 GetSecondCornerXZ(HexDirection direction) {
            return cornersXZ[(int)direction + 1];
        }

        public Vector2 GetFirstSolidCornerXZ (HexDirection direction) {
            return cornersXZ[(int)direction] * SolidFactor;
        }

        public Vector2 GetSecondSolidCornerXZ(HexDirection direction) {
            return cornersXZ[(int)direction + 1] * SolidFactor;
        }

        public Vector2 GetEdgeMidpointXZ(HexDirection direction) {
            return (cornersXZ[(int)direction] + cornersXZ[(int)direction + 1]) * 0.5f;
        }

        public Vector2 GetSolidEdgeMidpointXZ(HexDirection direction) {
            return (cornersXZ[(int)direction] * SolidFactor + cornersXZ[(int)direction + 1] * SolidFactor) * 0.5f;
        }

        #endregion

        #endregion

    }
}
