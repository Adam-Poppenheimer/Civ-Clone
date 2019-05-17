using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.HexMap {

    [CreateAssetMenu(menuName = "Civ Clone/Hex Map/Simulation Config")]
    public class HexMapSimulationConfig : ScriptableObject, IHexMapSimulationConfig {

        #region instance fields and properties

        #region from IHexGridConfig

        public int SlopeMoveCost {
            get { return _slopeMoveCost; }
        }
        [SerializeField] private int _slopeMoveCost = 0;

        public int CityMoveCost {
            get { return _cityMoveCost; }
        }
        [SerializeField] private int _cityMoveCost = 0;

        public float RoadMoveCostMultiplier {
            get { return _roadMoveCostMultiplier; }
        }
        [SerializeField] private float _roadMoveCostMultiplier = 0f;

        #endregion

        [SerializeField] private int BaseMoveCost = 0;

        [SerializeField] private YieldSummary GrasslandYield    = YieldSummary.Empty;
        [SerializeField] private YieldSummary PlainsYield       = YieldSummary.Empty;
        [SerializeField] private YieldSummary DesertYield       = YieldSummary.Empty;
        [SerializeField] private YieldSummary TundraYield       = YieldSummary.Empty;
        [SerializeField] private YieldSummary SnowYield         = YieldSummary.Empty;
        [SerializeField] private YieldSummary ShallowWaterYield = YieldSummary.Empty;
        [SerializeField] private YieldSummary DeepWaterYield    = YieldSummary.Empty;
        [SerializeField] private YieldSummary FreshWaterYield   = YieldSummary.Empty;
        [SerializeField] private YieldSummary FloodPlainsYield  = YieldSummary.Empty;

        [SerializeField] private YieldSummary HillsYield     = YieldSummary.Empty;
        [SerializeField] private YieldSummary MountainsYield = YieldSummary.Empty;

        [SerializeField] private YieldSummary ForestYield  = YieldSummary.Empty;
        [SerializeField] private YieldSummary JungleYield = YieldSummary.Empty;
        [SerializeField] private YieldSummary MarshYield  = YieldSummary.Empty;

        [SerializeField] private YieldSummary OasisYield = YieldSummary.Empty;

        [SerializeField] private int HillsMoveCost     = 0;
        [SerializeField] private int MountainsMoveCost = 0;

        [SerializeField] private int ForestMoveCost = 0;
        [SerializeField] private int JungleMoveCost = 0;
        [SerializeField] private int MarshMoveCost  = 0;

        [SerializeField] private int OasisMoveCost     = 0;
        [SerializeField] private int CityRuinsMoveCost = 0;
        #endregion

        #region instance methods

        #region from IHexMapConfig

        public bool DoesFeatureOverrideYield(CellFeature feature) {
            return feature == CellFeature.Oasis;
        }

        public YieldSummary GetYieldOfTerrain(CellTerrain terrain) {
            switch(terrain) {
                case CellTerrain.Grassland:    return GrasslandYield;
                case CellTerrain.Plains:       return PlainsYield;
                case CellTerrain.Desert:       return DesertYield;
                case CellTerrain.Tundra:       return TundraYield;
                case CellTerrain.Snow:         return SnowYield;
                case CellTerrain.ShallowWater: return ShallowWaterYield;
                case CellTerrain.DeepWater:    return DeepWaterYield;
                case CellTerrain.FreshWater:   return FreshWaterYield;
                case CellTerrain.FloodPlains:  return FloodPlainsYield;
                default: throw new NotImplementedException();
            }
        }

        public YieldSummary GetYieldOfShape(CellShape shape) {
            switch(shape) {
                case CellShape.Flatlands: return YieldSummary.Empty;
                case CellShape.Hills:     return HillsYield;
                case CellShape.Mountains: return MountainsYield;
                default: throw new NotImplementedException();
            }
        }

        public YieldSummary GetYieldOfVegetation(CellVegetation vegetation) {
            switch(vegetation) {
                case CellVegetation.None:   return YieldSummary.Empty;
                case CellVegetation.Forest: return ForestYield;
                case CellVegetation.Jungle: return JungleYield;
                case CellVegetation.Marsh:  return MarshYield;
                default: throw new NotImplementedException();
            }
        }

        public YieldSummary GetYieldOfFeature(CellFeature feature) {
            switch(feature) {
                case CellFeature.None:      return YieldSummary.Empty;
                case CellFeature.Oasis:     return OasisYield;
                case CellFeature.CityRuins: return YieldSummary.Empty;
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

        public int GetBaseMoveCostOfFeature(CellFeature feature) {
            switch(feature) {
                case CellFeature.None:      return 0;
                case CellFeature.Oasis:     return OasisMoveCost;
                case CellFeature.CityRuins: return CityRuinsMoveCost;
                default: throw new NotImplementedException(string.Format("No configured move cost for feature {0}", feature));
            }
        }

        #endregion

        #endregion

    }

}
