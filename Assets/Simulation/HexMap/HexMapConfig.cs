﻿using System;
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

        public int WaterTerrainIndex {
            get { return _waterTerrainIndex; }
        }
        [SerializeField] private int _waterTerrainIndex;

        public int MountainTerrainIndex {
            get { return _mountainTerrainIndex; }
        }
        [SerializeField] private int _mountainTerrainIndex;

        public int MaxElevation {
            get { return GetPeakElevationForShape(CellShape.Mountains) + GetFoundationElevationForTerrain(CellTerrain.Grassland); }
        }

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

        public ResourceSummary GetYieldOfTerrain(CellTerrain terrain) {
            switch(terrain) {
                case CellTerrain.Grassland:    return GrasslandYield;
                case CellTerrain.Plains:       return PlainsYield;
                case CellTerrain.Desert:       return DesertYield;
                case CellTerrain.Tundra:       return TundraYield;
                case CellTerrain.Snow:         return SnowYield;
                case CellTerrain.ShallowWater: return ShallowWaterYield;
                case CellTerrain.DeepWater:    return DeepWaterYield;
                case CellTerrain.FreshWater:   return FreshWaterYield;
                default: throw new NotImplementedException();
            }
        }

        public ResourceSummary GetYieldOfVegetation(CellVegetation feature) {
            switch(feature) {
                case CellVegetation.None:   return ResourceSummary.Empty;
                case CellVegetation.Forest: return ForestYield;
                case CellVegetation.Jungle: return JungleYield;
                case CellVegetation.Marsh:  return MarshYield;
                default: throw new NotImplementedException();
            }
        }

        public ResourceSummary GetYieldOfShape(CellShape shape) {
            switch(shape) {
                case CellShape.Flatlands: return ResourceSummary.Empty;
                case CellShape.Hills:     return HillsYield;
                case CellShape.Mountains: return MountainsYield;
                default: throw new NotImplementedException();
            }
        }

        public int GetBaseMoveCostOfTerrain(CellTerrain terrain) {
            return BaseMoveCost;
        }

        public int GetBaseMoveCostOfShape(CellShape shape) {
            switch(shape) {
                case CellShape.Flatlands: return 0;
                case CellShape.Hills:     return HillsMoveCost;
                case CellShape.Mountains: return MountainsMoveCost;
                default: throw new NotImplementedException();
            }
        }

        public int GetBaseMoveCostOfVegetation(CellVegetation feature) {
            switch(feature) {
                case CellVegetation.None:   return 0;
                case CellVegetation.Forest: return ForestMoveCost;
                case CellVegetation.Jungle: return JungleMoveCost;
                case CellVegetation.Marsh:  return MarshMoveCost;
                default: throw new NotImplementedException();
            }
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
