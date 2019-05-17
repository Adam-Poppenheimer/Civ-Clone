using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.Improvements;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapResources {

    [CreateAssetMenu(menuName = "Civ Clone/Map Resources/Resource")]
    public class ResourceDefinition : ScriptableObject, IResourceDefinition {

        #region instance fields and properties

        #region from ISpecialtyResourceDefinition

        public YieldSummary BonusYieldBase {
            get { return _bonusYieldBase; }
        }
        [SerializeField] private YieldSummary _bonusYieldBase = YieldSummary.Empty;

        public YieldSummary BonusYieldWhenImproved {
            get { return _bonusYieldWhenImproved; }
        }
        [SerializeField] private YieldSummary _bonusYieldWhenImproved = YieldSummary.Empty;

        public ResourceType Type {
            get { return _type; }
        }
        [SerializeField] private ResourceType _type = ResourceType.Bonus;

        public IImprovementTemplate Extractor {
            get { return _extractor; }
        }
        [SerializeField] private ImprovementTemplate _extractor = null;

        public float Score {
            get { return _score; }
        }
        [SerializeField, Range(0f, 20f)] private float _score = 0f;


        [SerializeField] private int GrasslandWeight    = 0;
        [SerializeField] private int PlainsWeight       = 0;
        [SerializeField] private int DesertWeight       = 0;
        [SerializeField] private int FloodPlainsWeight  = 0;
        [SerializeField] private int TundraWeight       = 0;
        [SerializeField] private int SnowWeight         = 0;
        [SerializeField] private int ShallowWaterWeight = 0;


        [SerializeField] private int FlatlandWeight = 0;
        [SerializeField] private int HillsWeight    = 0;

        [SerializeField] private int NoVegetationWeight = 0;
        [SerializeField] private int ForestWeight       = 0;
        [SerializeField] private int JungleWeight       = 0;
        [SerializeField] private int MarshWeight        = 0;



        public Transform AppearancePrefab {
            get { return _appearancePrefab; }
        }
        [SerializeField] private Transform _appearancePrefab = null;

        public Sprite Icon {
            get { return _icon; }
        }
        [SerializeField] private Sprite _icon = null;

        #endregion

        #endregion

        #region instance methods

        #region from IResourceDefinition

        public int GetWeightFromTerrain(CellTerrain terrain) {
            switch(terrain) {
                case CellTerrain.Grassland:    return GrasslandWeight;
                case CellTerrain.Plains:       return PlainsWeight;
                case CellTerrain.Desert:       return DesertWeight;
                case CellTerrain.FloodPlains:  return FloodPlainsWeight;
                case CellTerrain.Tundra:       return TundraWeight;
                case CellTerrain.Snow:         return SnowWeight;
                case CellTerrain.ShallowWater: return ShallowWaterWeight;
                case CellTerrain.DeepWater:    return -1000;
                case CellTerrain.FreshWater:   return -1000;
                default: throw new NotImplementedException();
            }
        }

        public int GetWeightFromShape(CellShape shape) {
            switch(shape) {
                case CellShape.Flatlands: return FlatlandWeight;
                case CellShape.Hills:     return HillsWeight;
                case CellShape.Mountains: return -1000;
                default: throw new NotImplementedException();
            }
        }

        public int GetWeightFromVegetation(CellVegetation vegetation) {
            switch(vegetation) {
                case CellVegetation.None:   return NoVegetationWeight;
                case CellVegetation.Forest: return ForestWeight;
                case CellVegetation.Jungle: return JungleWeight;
                case CellVegetation.Marsh:  return MarshWeight;
                default: throw new NotImplementedException();
            }
        }

        #endregion

        #endregion

    }

}
