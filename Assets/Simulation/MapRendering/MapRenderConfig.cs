using System;
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

        public float NoiseScale {
            get { return _noiseScale; }
        }
        [SerializeField] private float _noiseScale;

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

        public float OuterSolidFactor {
            get { return _outerSolidFactor; }
        }
        [SerializeField] private float _outerSolidFactor;

        public float InnerSolidFactor {
            get { return _innerSolidFactor; }
        }
        [SerializeField] private float _innerSolidFactor;

        public float BlendFactor {
            get { return 1f - OuterSolidFactor; }
        }

        public IEnumerable<Vector3> Corners {
            get { return corners; }
        }
        private Vector3[] corners;

        

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
        [SerializeField] private float _terrainMaxY;

        public int TerrainAlphamapResolution  {
            get { return _terrainAlphamapResolution; }
        }
        [SerializeField] private int _terrainAlphamapResolution;

        public int TerrainHeightmapResolution {
            get { return _terrainHeightmapResolution; }
        }
        [SerializeField] private int _terrainHeightmapResolution;


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


        public float WaterY {
            get { return WaterSurfaceElevation * TerrainMaxY; }
        }


        public INoiseTexture FlatlandsElevationHeightmap {
            get {
                if(_flatlandsElevationHeightmap_Wrapped == null) {
                    _flatlandsElevationHeightmap_Wrapped = new NoiseTexture(_flatlandsElevationHeightmap);
                }
                return _flatlandsElevationHeightmap_Wrapped;
            }
        }
        private INoiseTexture _flatlandsElevationHeightmap_Wrapped;
        [SerializeField] private Texture2D _flatlandsElevationHeightmap;

        public INoiseTexture HillsElevationHeightmap  {
            get {
                if(_hillsElevationHeightmap_Wrapped == null) {
                    _hillsElevationHeightmap_Wrapped = new NoiseTexture(_hillsElevationHeightmap);
                }
                return _hillsElevationHeightmap_Wrapped;
            }
        }
        private INoiseTexture _hillsElevationHeightmap_Wrapped;
        [SerializeField] private Texture2D _hillsElevationHeightmap;


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


        public float RiverWidth {
            get { return _riverWidth; }
        }
        [SerializeField, Range(0f, 10f)] private float _riverWidth;

        public float RiverCurveStrength {
            get { return _riverCurveStrength; }
        }
        [SerializeField] private float _riverCurveStrength;

        public int RiverQuadsPerCurve {
            get { return _riverQuadsPerCurve; }
        }
        [SerializeField, Range(1f, 100f)] private int _riverQuadsPerCurve;

        #endregion

        private Vector2[] CornersXZ;

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

            CornersXZ = new Vector2[] {
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
            return corners[(int)direction] * OuterSolidFactor;
        }

        public Vector3 GetSecondSolidCorner(HexDirection direction) {
            return corners[(int)direction + 1] * OuterSolidFactor;
        }

        public Vector3 GetEdgeMidpoint(HexDirection direction) {
            return (corners[(int)direction] + corners[(int)direction + 1]) * 0.5f;
        }

        public Vector3 GetSolidEdgeMidpoint(HexDirection direction) {
            return (corners[(int)direction] * OuterSolidFactor + corners[(int)direction + 1] * OuterSolidFactor) * 0.5f;
        }



        public Vector2 GetFirstCornerXZ(HexDirection direction) {
            return CornersXZ[(int)direction];
        }

        public Vector2 GetSecondCornerXZ(HexDirection direction) {
            return CornersXZ[(int)direction + 1];
        }

        public Vector2 GetFirstSolidCornerXZ (HexDirection direction) {
            return CornersXZ[(int)direction] * OuterSolidFactor;
        }

        public Vector2 GetSecondSolidCornerXZ(HexDirection direction) {
            return CornersXZ[(int)direction + 1] * OuterSolidFactor;
        }

        public Vector2 GetEdgeMidpointXZ(HexDirection direction) {
            return (CornersXZ[(int)direction] + CornersXZ[(int)direction + 1]) * 0.5f;
        }

        public Vector2 GetSolidEdgeMidpointXZ(HexDirection direction) {
            return (CornersXZ[(int)direction] * OuterSolidFactor + CornersXZ[(int)direction + 1] * OuterSolidFactor) * 0.5f;
        }

        #endregion

        #endregion

    }
}
