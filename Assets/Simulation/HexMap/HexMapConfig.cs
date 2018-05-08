using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.HexMap {

    [CreateAssetMenu(fileName = "New Hex Map Config", menuName = "Civ Clone/Hex Map Config")]
    public class HexMapConfig : ScriptableObject, IHexMapConfig {

        #region instance fields and properties

        #region from IHexGridConfig

        public int RandomSeed {
            get { return _randomSeed; }
        }
        [SerializeField] private int _randomSeed;

        public int SlopeMoveCost {
            get { return _slopeMoveCost; }
        }
        [SerializeField] private int _slopeMoveCost;

        public int CityMoveCost {
            get { return _cityMoveCost; }
        }
        [SerializeField] private int _cityMoveCost;

        public float RoadMoveCostMultiplier {
            get { return _roadMoveCostMultiplier; }
        }
        [SerializeField] private float _roadMoveCostMultiplier;

        public int WaterLevel {
            get { return _waterLevel; }
        }
        [SerializeField] private int _waterLevel;

        #endregion

        [SerializeField] private int BaseMoveCost;

        [SerializeField] private ResourceSummary GrasslandYield;
        [SerializeField] private ResourceSummary PlainsYield;
        [SerializeField] private ResourceSummary DesertYield;
        [SerializeField] private ResourceSummary TundraYield;
        [SerializeField] private ResourceSummary SnowYield;
        [SerializeField] private ResourceSummary ShallowWaterYield;
        [SerializeField] private ResourceSummary DeepWaterYield;
        [SerializeField] private ResourceSummary FreshWaterYield;

        [SerializeField] private ResourceSummary HillsYield;
        [SerializeField] private ResourceSummary MountainsYield;

        [SerializeField] private ResourceSummary ForestYield;
        [SerializeField] private ResourceSummary JungleYield;
        [SerializeField] private ResourceSummary MarshYield;

        [SerializeField] private int HillsMoveCost;
        [SerializeField] private int MountainsMoveCost;

        [SerializeField] private int ForestMoveCost;
        [SerializeField] private int JungleMoveCost;
        [SerializeField] private int MarshMoveCost;

        [SerializeField] private int FreshWaterFoundationElevation;
        [SerializeField] private int ShallowWaterFoundationElevation;
        [SerializeField] private int DeepWaterFoundationElevation;
        [SerializeField] private int LandBaseFoundationElevation;

        [SerializeField] private int HillsEdgeElevation;
        [SerializeField] private int MountainsEdgeElevation;

        [SerializeField] private int HillsPeakElevation;
        [SerializeField] private int MountainsPeakElevation;

        #endregion

        #region instance methods

        #region from IHexMapConfig

        public ResourceSummary GetYieldOfTerrain(TerrainType terrain) {
            switch(terrain) {
                case TerrainType.Grassland:    return GrasslandYield;
                case TerrainType.Plains:       return PlainsYield;
                case TerrainType.Desert:       return DesertYield;
                case TerrainType.Tundra:       return TundraYield;
                case TerrainType.Snow:         return SnowYield;
                case TerrainType.ShallowWater: return ShallowWaterYield;
                case TerrainType.DeepWater:    return DeepWaterYield;
                case TerrainType.FreshWater:   return FreshWaterYield;
                default: throw new NotImplementedException();
            }
        }

        public ResourceSummary GetYieldOfFeature(TerrainFeature feature) {
            switch(feature) {
                case TerrainFeature.None:   return ResourceSummary.Empty;
                case TerrainFeature.Forest: return ForestYield;
                case TerrainFeature.Jungle: return JungleYield;
                case TerrainFeature.Marsh:  return MarshYield;
                default: throw new NotImplementedException();
            }
        }

        public ResourceSummary GetYieldOfShape(TerrainShape shape) {
            switch(shape) {
                case TerrainShape.Flatlands: return ResourceSummary.Empty;
                case TerrainShape.Hills:     return HillsYield;
                case TerrainShape.Mountains: return MountainsYield;
                default: throw new NotImplementedException();
            }
        }

        public int GetBaseMoveCostOfTerrain(TerrainType terrain) {
            return BaseMoveCost;
        }

        public int GetBaseMoveCostOfShape(TerrainShape shape) {
            switch(shape) {
                case TerrainShape.Flatlands: return 0;
                case TerrainShape.Hills:     return HillsMoveCost;
                case TerrainShape.Mountains: return MountainsMoveCost;
                default: throw new NotImplementedException();
            }
        }

        public int GetBaseMoveCostOfFeature(TerrainFeature feature) {
            switch(feature) {
                case TerrainFeature.None:   return 0;
                case TerrainFeature.Forest: return ForestMoveCost;
                case TerrainFeature.Jungle: return JungleMoveCost;
                case TerrainFeature.Marsh:  return MarshMoveCost;
                default: throw new NotImplementedException();
            }
        }

        public int GetFoundationElevationForTerrain(TerrainType terrain) {
            switch(terrain) {
                case TerrainType.FreshWater:   return FreshWaterFoundationElevation;
                case TerrainType.ShallowWater: return ShallowWaterFoundationElevation;
                case TerrainType.DeepWater:    return DeepWaterFoundationElevation;
                default:                       return LandBaseFoundationElevation;
            }
        }

        public int GetEdgeElevationForShape(TerrainShape shape) {
            switch(shape) {
                case TerrainShape.Flatlands: return 0;
                case TerrainShape.Hills:     return HillsEdgeElevation;
                case TerrainShape.Mountains: return MountainsEdgeElevation;
                default: throw new NotImplementedException();
            }
        }

        public int GetPeakElevationForShape(TerrainShape shape) {
            switch(shape) {
                case TerrainShape.Flatlands: return 0;
                case TerrainShape.Hills:     return HillsPeakElevation;
                case TerrainShape.Mountains: return MountainsPeakElevation;
                default: throw new NotImplementedException();
            }
        }

        #endregion

        #endregion

    }

}
