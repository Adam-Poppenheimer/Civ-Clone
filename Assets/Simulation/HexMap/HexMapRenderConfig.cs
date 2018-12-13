using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.HexMap {

    [CreateAssetMenu(menuName = "Civ Clone/Hex Map/Render Config")]
    public class HexMapRenderConfig : ScriptableObject, IHexMapRenderConfig {

        #region instance fields and properties

        #region from IHexMapRenderConfig

        public int RandomSeed {
            get { return _randomSeed; }
        }
        [SerializeField] private int _randomSeed;


        public Texture2D NoiseSource {
            get { return _noiseSource; }
        }
        [SerializeField] private Texture2D _noiseSource;

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


        public int WaterLevel {
            get { return _waterLevel; }
        }
        [SerializeField] private int _waterLevel = 3;

        public int MaxElevation {
            get { return GetPeakElevationForShape(CellShape.Mountains) + GetFoundationElevationForTerrain(CellTerrain.Grassland);; }
        }

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


        public float ElevationStep {
            get { return _elevationStep; }
        }
        [SerializeField] private float _elevationStep;


        public int TerracesPerSlope {
            get { return _terracesPerSlope; }
        }
        [SerializeField] private int _terracesPerSlope;


        public int TerraceSteps {
            get { return TerracesPerSlope * 2 + 1; }
        }


        public float HorizontalTerraceStepSize {
            get { return 1f / TerraceSteps; }
        }

        public float VerticalTerraceStepSize {
            get { return 1f / (TerracesPerSlope + 1); }
        }


        public float CellPerturbStrengthXZ {
            get { return _cellPerturbStrengthXZ; }
        }
        [SerializeField] private float _cellPerturbStrengthXZ;

        public float CellPerturbStrengthY {
            get { return _cellPerturbStrengthY; }
        }
        [SerializeField] private float _cellPerturbStrengthY;


        public float MinHillPerturbation {
            get { return _minHillPerturbation; }
        }
        [SerializeField] private float _minHillPerturbation;

        
        public int ChunkSizeX {
            get { return _chunkSizeX; }
        }
        [SerializeField] private int _chunkSizeX;

        public int ChunkSizeZ {
            get { return _chunkSizeZ; }
        }
        [SerializeField] private int _chunkSizeZ;


        public float StreamBedElevationOffset {
            get { return _streamBedElevationOffset; }
        }
        [SerializeField] private float _streamBedElevationOffset;


        public float OceanElevationOffset {
            get { return _oceanElevationOffset; }
        }
        [SerializeField] private float _oceanElevationOffset;

        public float RiverElevationOffset {
            get { return _riverElevationOffset; }
        }
        [SerializeField] private float _riverElevationOffset;


        public float WaterFactor {
            get { return _waterFactor; }
        }
        [SerializeField] private float _waterFactor;

        public float WaterBlendFactor {
            get { return 1f - WaterFactor; }
        }


        public float RiverTroughEndpointPullback {
            get { return _riverTroughEndpointPullback; }
        }
        [SerializeField] private float _riverTroughEndpointPullback;


        public float RiverPortU {
            get { return _riverPortU; }
        }
        [SerializeField] private float _riverPortU;

        public float RiverStarboardU {
            get { return _riverStarboardU; }
        }
        [SerializeField] private float _riverStarboardU;


        public float RiverEndpointV {
            get { return _riverEndpointV; }
        }
        [SerializeField] private float _riverEndpointV;

        public float RiverEdgeStartV {
            get { return _riverEdgeStartV; }
        }
        [SerializeField] private float _riverEdgeStartV;

        public float RiverEdgeStepV {
            get { return _riverEdgeStepV; }
        }
        [SerializeField] private float _riverEdgeStepV;

        public float RiverEdgeEndV {
            get { return RiverEdgeStartV + 4 * RiverEdgeStepV; }
        }

        
        public float RiverConfluenceV {
            get { return _riverConfluenceV; }
        }
        [SerializeField] private float _riverConfluenceV;

        public float EstuaryWaterfallV {
            get { return _estuaryWaterfallV; }
        }
        [SerializeField] private float _estuaryWaterfallV;

        public float RiverWaterfallV {
            get { return _riverWaterfallV; }
        }
        [SerializeField] private float _riverWaterfallV;


        public float CornerWaterfallTroughProtrusion {
            get { return _cornerWaterfallTroughProtrusion; }
        }
        [SerializeField] private float _cornerWaterfallTroughProtrusion;


        public float OasisWaterLerp {
            get { return _oasisWaterLerp; }
        }
        [SerializeField] private float _oasisWaterLerp;

        public float OasisVerdantEdgeLerp {
            get { return _oasisVerdantEdgeLerp; }
        }
        [SerializeField] private float _oasisVerdantEdgeLerp;

        public int WaterTerrainIndex{
            get { return _waterTerrainIndex; }
        }
        [SerializeField] private int _waterTerrainIndex = 5;

        public int MountainTerrainIndex {
            get { return _mountainTerrainIndex; }
        }
        [SerializeField] private int _mountainTerrainIndex = 6;

        #endregion

        [SerializeField] private int FreshWaterFoundationElevation = 2;
        [SerializeField] private int ShallowWaterFoundationElevation = 2;
        [SerializeField] private int DeepWaterFoundationElevation = 0;
        [SerializeField] private int LandBaseFoundationElevation = 3;

        [SerializeField] private int HillsEdgeElevation = 1;
        [SerializeField] private int MountainsEdgeElevation = 1;

        [SerializeField] private int HillsPeakElevation = 2;
        [SerializeField] private int MountainsPeakElevation = 5;

        private Vector3[] Corners {
            get {
                if(_corners == null) {
                    _corners = new Vector3[] {
                        new Vector3(0f, 0f,  OuterRadius),
                        new Vector3(InnerRadius, 0f,  0.5f * OuterRadius),
                        new Vector3(InnerRadius, 0f, -0.5f * OuterRadius),
                        new Vector3(0f, 0f, -OuterRadius),
                        new Vector3(-InnerRadius, 0f, -0.5f * OuterRadius),
                        new Vector3(-InnerRadius, 0f,  0.5f * OuterRadius),
                        new Vector3(0f, 0f, OuterRadius)
                    };
                }
                return _corners;
            }
        } 
        private Vector3[] _corners;

        #endregion

        #region instance methods

        #region from IHexMapRenderConfig

        public Vector3 GetFirstCorner(HexDirection direction) {
            return Corners[(int)direction];
        }

        public Vector3 GetSecondCorner(HexDirection direction) {
            return Corners[(int)direction + 1];
        }

        public Vector3 GetFirstOuterSolidCorner(HexDirection direction) {
            return Corners[(int)direction] * OuterSolidFactor;
        }

        public Vector3 GetSecondOuterSolidCorner(HexDirection direction) {
            return Corners[(int)direction + 1] * OuterSolidFactor;
        }

        public Vector3 GetFirstInnerSolidCorner(HexDirection direction) {
            return Corners[(int)direction] * InnerSolidFactor;
        }

        public Vector3 GetSecondInnerSolidCorner(HexDirection direction) {
            return Corners[(int)direction + 1] * InnerSolidFactor;
        }

        public Vector3 TerraceLerp(Vector3 a, Vector3 b, int step) {
            float horizontalDelta = step * HorizontalTerraceStepSize;
            a.x += (b.x - a.x) * horizontalDelta;
            a.z += (b.z - a.z) * horizontalDelta;

            float verticalDelta = ((step + 1) / 2) * VerticalTerraceStepSize;
            a.y += (b.y - a.y) * verticalDelta;

            return a;
        }

        public Color TerraceLerp(Color a, Color b, int step) {
            float h = step * HorizontalTerraceStepSize;
            return Color.Lerp(a, b, h);
        }

        public float TerraceLerp(float a, float b, int step) {
            float h = step * HorizontalTerraceStepSize;
            return Mathf.Lerp(a, b, h);
        }

        public EdgeVertices TerraceLerp(EdgeVertices a, EdgeVertices b, int step) {
            EdgeVertices result;

            result.V1 = TerraceLerp(a.V1, b.V1, step);
            result.V2 = TerraceLerp(a.V2, b.V2, step);
            result.V3 = TerraceLerp(a.V3, b.V3, step);
            result.V4 = TerraceLerp(a.V4, b.V4, step);
            result.V5 = TerraceLerp(a.V5, b.V5, step);

            return result;
        }

        public Vector3 GetOuterEdgeMiddle(HexDirection direction) {
            return (Corners[(int)direction] + Corners[(int)direction + 1]) * 0.5f;
        }

        public Vector3 GetFirstWaterCorner(HexDirection direction) {
            return Corners[(int)direction] * WaterFactor;
        }

        public Vector3 GetSecondWaterCorner(HexDirection direction) {
            return Corners[(int)direction + 1] * WaterFactor;
        }

        public float GetRiverEdgeV(int vertex) {
            return RiverEdgeStartV + vertex * RiverEdgeStepV;
        }

        public int GetFoundationElevationForTerrain(CellTerrain terrain) {
            switch(terrain) {
                case CellTerrain.FreshWater:   return FreshWaterFoundationElevation;
                case CellTerrain.ShallowWater: return ShallowWaterFoundationElevation;
                case CellTerrain.DeepWater:    return DeepWaterFoundationElevation;
                default:                       return LandBaseFoundationElevation;
            }
        }

        public int GetEdgeElevationForShape(CellShape shape) {
            switch(shape) {
                case CellShape.Flatlands: return 0;
                case CellShape.Hills:     return HillsEdgeElevation;
                case CellShape.Mountains: return MountainsEdgeElevation;
                default: throw new NotImplementedException();
            }
        }

        public int GetPeakElevationForShape(CellShape shape) {
            switch(shape) {
                case CellShape.Flatlands: return 0;
                case CellShape.Hills:     return HillsPeakElevation;
                case CellShape.Mountains: return MountainsPeakElevation;
                default: throw new NotImplementedException();
            }
        }

        #endregion

        #endregion

    }
}
