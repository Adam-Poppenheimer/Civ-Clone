using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    [CreateAssetMenu(menuName = "Civ Clone/Map Rendering/Config")]
    public class MapRenderConfig : ScriptableObject, IMapRenderConfig {

        #region instance fields and properties

        #region from IMapRenderConfig

        public int RandomSeed {
            get { return _randomSeed; }
        }
        [SerializeField] private int _randomSeed;

        public float NoiseScale {
            get { return _noiseScale; }
        }
        [SerializeField] private float _noiseScale;

        public INoiseTexture GenericNoiseSource {
            get {
                if(_genericNoiseSource_Wrapped == null) {
                    _genericNoiseSource_Wrapped = new NoiseTexture(_genericNoiseSource);
                }
                return _genericNoiseSource_Wrapped;
            }
        }
        private INoiseTexture _genericNoiseSource_Wrapped;
        [SerializeField] private Texture2D _genericNoiseSource;

        public INoiseTexture FlatlandsElevationNoiseSource {
            get {
                if(_flatlandsElevationNoiseSource_Wrapped == null) {
                    _flatlandsElevationNoiseSource_Wrapped = new NoiseTexture(_flatlandsElevationNoiseSource);
                }
                return _flatlandsElevationNoiseSource_Wrapped;
            }
        }
        private INoiseTexture _flatlandsElevationNoiseSource_Wrapped;
        [SerializeField] private Texture2D _flatlandsElevationNoiseSource;

        public INoiseTexture HillsElevationNoiseSource  {
            get {
                if(_hillsElevationNoiseSource_Wrapped == null) {
                    _hillsElevationNoiseSource_Wrapped = new NoiseTexture(_hillsElevationNoiseSource);
                }
                return _hillsElevationNoiseSource_Wrapped;
            }
        }
        private INoiseTexture _hillsElevationNoiseSource_Wrapped;
        [SerializeField] private Texture2D _hillsElevationNoiseSource;        

        public int NoiseHashGridSize {
            get { return _noiseHashGridSize; }
        }
        [SerializeField] private int _noiseHashGridSize;

        public float NoiseHashGridScale {
            get { return _noiseHashGridScale; }
        }
        [SerializeField] private float _noiseHashGridScale;

        public float CellPerturbStrengthXZ {
            get { return _cellPerturbStrengthXZ; }
        }
        [SerializeField] private float _cellPerturbStrengthXZ;

        public float FlatlandsElevationNoiseStrength {
            get { return _flatlandsElevationNoiseStrength; }
        }
        [SerializeField, Range(0f, 1f)] private float _flatlandsElevationNoiseStrength;

        public float HillsElevationNoiseStrength {
            get { return _hillsElevationNoiseStrength; }
        }
        [SerializeField, Range(0f, 1f)] private float _hillsElevationNoiseStrength;



        public float OuterToInner {
            get { return 0.866025404f; }
        }

        public float InnerToOuter {
            get { return 1f / OuterToInner; }
        }

        public float OuterRadius {
            get { return _outerRadius; }
        }
        [SerializeField] private float _outerRadius;

        public float InnerRadius {
            get { return OuterRadius * OuterToInner; }
        }

        public float SolidFactor {
            get { return _solidFactor; }
        }
        [SerializeField] private float _solidFactor;

        public float BlendFactor {
            get { return 1f - SolidFactor; }
        }

        public IEnumerable<Vector3> Corners {
            get { return corners; }
        }
        private Vector3[] corners;

        public IEnumerable<Vector2> CornersXZ {
            get { return cornersXZ; }
        }
        private Vector2[] cornersXZ;

        

        public float ChunkWidth {
            get { return _chunkWidth; }
        }
        [SerializeField] private int _chunkWidth;

        public float ChunkHeight {
            get { return _chunkHeight; }
        }
        [SerializeField] private float _chunkHeight;

        public float TerrainMaxY {
            get { return _terrainMaxY; }
        }
        [SerializeField, Range(10, 100)] private float _terrainMaxY;

        public int TerrainAlphamapResolution  {
            get { return _terrainAlphamapResolution; }
        }
        [SerializeField] private int _terrainAlphamapResolution;

        public int TerrainHeightmapResolution {
            get { return _terrainHeightmapResolution; }
        }
        [SerializeField] private int _terrainHeightmapResolution;

        public int TerrainBasemapResolution {
            get { return _terrainBasemapResolution; }
        }
        [SerializeField] private int _terrainBasemapResolution;

        public float TerrainBasemapDistance {
            get { return _terrainBasemapDistance; }
        }
        [SerializeField] private float _terrainBasemapDistance;

        public bool TerrainCastsShadows {
            get { return _terrainCastsShadows; }
        }
        [SerializeField] private bool _terrainCastsShadows;

        public int TerrainHeightmapPixelError {
            get { return _terrainHeightmapPixelError; }
        }
        [SerializeField, Range(1, 1000)] private int _terrainHeightmapPixelError;

        public LayerMask MapCollisionMask {
            get { return _mapCollisionMask; }
        }
        [SerializeField] private LayerMask _mapCollisionMask;

        public Material TerrainMaterialTemplate {
            get { return _terrainMaterialTemplate; }
        }
        [SerializeField] private Material _terrainMaterialTemplate;

        public IEnumerable<Texture2D> MapTextures {
            get { return _mapTextures; }
        }
        [SerializeField] private List<Texture2D> _mapTextures;

        public int SeaFloorTextureIndex {
            get { return _seaFloorTextureIndex; }
        }
        [SerializeField] private int _seaFloorTextureIndex;

        public int MountainTextureIndex {
            get { return _mountainTextureIndex; }
        }
        [SerializeField] private int _mountainTextureIndex;

        public float[] RiverAlphamap {
            get { return _riverAlphamap; }
        }
        [SerializeField] private float[] _riverAlphamap;

        public float[] FloodPlainsAlphamap {
            get { return _floodPlainsAlphamap; }
        }
        [SerializeField] private float[] _floodPlainsAlphamap;



        public float WaterSurfaceElevation  {
            get { return _waterSurfaceElevation; }
        }
        [SerializeField, Range(0f, 1f)] private float _waterSurfaceElevation;

        public float SeaFloorElevation  {
            get { return _seaFloorElevation; }
        }
        [SerializeField, Range(0f, 1f)] private float _seaFloorElevation;

        public float FlatlandsBaseElevation  {
            get { return _flatlandsBaseElevation; }
        }
        [SerializeField, Range(0f, 1f)] private float _flatlandsBaseElevation;

        public float HillsBaseElevation  {
            get { return _hillsBaseElevation; }
        }
        [SerializeField, Range(0f, 1f)] private float _hillsBaseElevation;

        public float MountainPeakElevation  {
            get { return _mountainPeakElevation; }
        }
        [SerializeField, Range(0f, 1f)] private float _mountainPeakElevation;

        public float MountainRidgeElevation  {
            get { return _mountainRidgeElevation; }
        }
        [SerializeField, Range(0f, 1f)] private float _mountainRidgeElevation;

        public float RiverTroughElevation  {
            get { return _riverTroughElevation; }
        }
        [SerializeField, Range(0f, 1f)] private float _riverTroughElevation;


        public float WaterY {
            get { return WaterSurfaceElevation * TerrainMaxY; }
        }

        public Color ShallowWaterColor {
            get { return _shallowWaterColor; }
        }
        [SerializeField] private Color _shallowWaterColor;

        public Color DeepWaterColor {
            get { return _deepWaterColor; }
        }
        [SerializeField] private Color _deepWaterColor;

        public Color FreshWaterColor {
            get { return _freshWaterColor; }
        }
        [SerializeField] private Color _freshWaterColor;

        public Color RiverWaterColor {
            get { return _riverWaterColor; }
        }
        [SerializeField] private Color _riverWaterColor;


        public float RiverMaxInnerWidth {
            get { return _riverMaxInnerWidth; }
        }
        [SerializeField, Range(0f, 10f)] private float _riverMaxInnerWidth;

        public float RiverCurveStrength {
            get { return _riverCurveStrength; }
        }
        [SerializeField] private float _riverCurveStrength;

        public float RiverWideningRate {
            get { return _riverWideningRate; }
        }
        [SerializeField, Range(0f, 100f)] private float _riverWideningRate;

        public float RiverWidthNoise {
            get { return _riverWidthNoise; }
        }
        [SerializeField, Range(0f, 1f)] private float _riverWidthNoise = 0f;

        public float RiverBankWidth {
            get { return _riverBankWidth; }
        }
        [SerializeField, Range(0f, 5f)] private float _riverBankWidth = 0f;

        public int RiverCurvesForMaxWidth {
            get { return _riverCurvesForMaxWidth; }
        }
        [SerializeField, Range(0f, 30f)] private int _riverCurvesForMaxWidth = 10;

        public int RiverQuadsPerCurve {
            get { return _riverQuadsPerCurve; }
        }
        [SerializeField, Range(1f, 100f)] private int _riverQuadsPerCurve;

        public float RiverFlowSpeed {
            get { return _riverFlowSpeed; }
        }
        [SerializeField] private float _riverFlowSpeed;




        public float CultureWidthPercent {
            get { return _cultureWidthPercent; }
        }
        [SerializeField, Range(0f, 1f)] private float _cultureWidthPercent;



        public HexMeshData StandingWaterData {
            get { return _standingWaterData; }
        }
        [SerializeField] private HexMeshData _standingWaterData;

        public HexMeshData RiverSurfaceData {
            get { return _riverSurfaceData; }
        }
        [SerializeField] private HexMeshData _riverSurfaceData;

        public HexMeshData RiverBankData {
            get { return _riverBankData; }
        }
        [SerializeField] private HexMeshData _riverBankData;

        public HexMeshData CultureData {
            get { return _cultureData; }
        }
        [SerializeField] private HexMeshData _cultureData;

        public HexMeshData FarmlandData {
            get { return _farmlandData; }
        }
        [SerializeField] private HexMeshData _farmlandData;

        public HexMeshData RoadData {
            get { return _roadData; }
        }
        [SerializeField] private HexMeshData _roadData;

        public HexMeshData MarshWaterData {
            get { return _marshWaterData; }
        }
        [SerializeField] private HexMeshData _marshWaterData;

        public HexMeshData OasisWaterData {
            get { return _oasisWaterData; }
        }
        [SerializeField] private HexMeshData _oasisWaterData;

        public HexMeshData OasisLandData {
            get { return _oasisLandData; }
        }
        [SerializeField] private HexMeshData _oasisLandData;

        public HexMeshData OrientationMeshData {
            get { return _orientationMeshData; }
        }
        [SerializeField] private HexMeshData _orientationMeshData;

        public HexMeshData WeightsMeshData {
            get { return _weightsMeshData; }
        }
        [SerializeField] private HexMeshData _weightsMeshData;



        public ReadOnlyCollection<Color> FarmColors {
            get { return _farmColors.AsReadOnly(); }
        }
        [SerializeField] private List<Color> _farmColors;

        public float FarmPatchMinWidth {
            get { return _farmPatchMinWidth; }
        }
        [SerializeField, Range(1f, 20f)] private float _farmPatchMinWidth;

        public float FarmPatchMaxWidth {
            get { return _farmPatchMaxWidth; }
        }
        [SerializeField, Range(1f, 20f)] private float _farmPatchMaxWidth;



        public int RoadQuadsPerCurve {
            get { return _roadQuadsPerCurve; }
        }
        [SerializeField] private int _roadQuadsPerCurve;

        public float RoadWidth {
            get { return _roadWidth; }
        }
        [SerializeField] private float _roadWidth;

        public float RoadVRepeatLength {
            get { return _roadVRepeatLength; }
        }
        [SerializeField] private float _roadVRepeatLength;



        public int OasisBoundarySegments {
            get { return _oasisBoundarySegments; }
        }
        [SerializeField] private int _oasisBoundarySegments;

        public float OasisWaterRadius {
            get { return _oasisWaterRadius; }
        }
        [SerializeField] private float _oasisWaterRadius;

        public float OasisLandWidth {
            get { return _oasisLandWidth; }
        }
        [SerializeField] private float _oasisLandWidth;



        public RenderTextureData TerrainBakeTextureData {
            get { return _terrainBakeTextureData; }
        }
        [SerializeField] private RenderTextureData _terrainBakeTextureData;

        public RenderTextureData OrientationTextureData {
            get { return _orientationTextureData; }
        }
        [SerializeField] private RenderTextureData _orientationTextureData;


        public Shader TerrainBakeOcclusionShader {
            get { return _terrainBakeOcclusionShader; }
        }
        [SerializeField] private Shader _terrainBakeOcclusionShader;

        public Shader RiverWeightShader {
            get { return _riverWeightShader; }
        }
        [SerializeField] private Shader _riverWeightShader;

        #endregion

        #endregion

        #region instance methods

        #region Unity messages

        private void OnEnable() {
            SetCorners();
        }

        private void Awake() {
            SetCorners();
        }

        private void OnValidate() {
            SetCorners();
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
