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

        public float MaxElevation {
            get { return _maxElevation; }
        }
        [SerializeField] private float _maxElevation;

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
                new Vector3(0f, 0f,  OuterRadius),
                new Vector3(InnerRadius, 0f,  0.5f * OuterRadius),
                new Vector3(InnerRadius, 0f, -0.5f * OuterRadius),
                new Vector3(0f, 0f, -OuterRadius),
                new Vector3(-InnerRadius, 0f, -0.5f * OuterRadius),
                new Vector3(-InnerRadius, 0f,  0.5f * OuterRadius),
                new Vector3(0f, 0f, OuterRadius)
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

        public Vector3 GetFirstOuterSolidCorner(HexDirection direction) {
            return corners[(int)direction] * OuterSolidFactor;
        }

        public Vector3 GetSecondOuterSolidCorner(HexDirection direction) {
            return corners[(int)direction + 1] * OuterSolidFactor;
        }

        public Vector3 GetOuterEdgeMidpoint(HexDirection direction) {
            return (corners[(int)direction] + corners[(int)direction + 1]) * 0.5f;
        }

        #endregion

        #endregion

    }
}
