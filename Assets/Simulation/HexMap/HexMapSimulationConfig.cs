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
        [SerializeField] private int _slopeMoveCost;

        public int CityMoveCost {
            get { return _cityMoveCost; }
        }
        [SerializeField] private int _cityMoveCost;

        public float RoadMoveCostMultiplier {
            get { return _roadMoveCostMultiplier; }
        }
        [SerializeField] private float _roadMoveCostMultiplier;

        #endregion

        [SerializeField] private int BaseMoveCost;

        [SerializeField] private YieldSummary GrasslandYield;
        [SerializeField] private YieldSummary PlainsYield;
        [SerializeField] private YieldSummary DesertYield;
        [SerializeField] private YieldSummary TundraYield;
        [SerializeField] private YieldSummary SnowYield;
        [SerializeField] private YieldSummary ShallowWaterYield;
        [SerializeField] private YieldSummary DeepWaterYield;
        [SerializeField] private YieldSummary FreshWaterYield;
        [SerializeField] private YieldSummary FloodPlainsYield;

        [SerializeField] private YieldSummary HillsYield;
        [SerializeField] private YieldSummary MountainsYield;

        [SerializeField] private YieldSummary ForestYield;
        [SerializeField] private YieldSummary JungleYield;
        [SerializeField] private YieldSummary MarshYield;

        [SerializeField] private YieldSummary OasisYield;

        [SerializeField] private int HillsMoveCost;
        [SerializeField] private int MountainsMoveCost;

        [SerializeField] private int ForestMoveCost;
        [SerializeField] private int JungleMoveCost;
        [SerializeField] private int MarshMoveCost;

        [SerializeField] private int OasisMoveCost;
        [SerializeField] private int CityRuinsMoveCost;
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
