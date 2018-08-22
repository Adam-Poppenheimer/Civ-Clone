using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.Improvements;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapResources {

    [CreateAssetMenu(menuName = "Civ Clone/Specialty Resource")]
    public class ResourceDefinition : ScriptableObject, IResourceDefinition {

        #region instance fields and properties

        #region from ISpecialtyResourceDefinition

        public YieldSummary BonusYieldBase {
            get { return _bonusYieldBase; }
        }
        [SerializeField] private YieldSummary _bonusYieldBase;

        public YieldSummary BonusYieldWhenImproved {
            get { return _bonusYieldWhenImproved; }
        }
        [SerializeField] private YieldSummary _bonusYieldWhenImproved;

        public ResourceType Type {
            get { return _type; }
        }
        [SerializeField] private ResourceType _type;

        public IImprovementTemplate Extractor {
            get { return _extractor; }
        }
        [SerializeField] private ImprovementTemplate _extractor;


        [SerializeField] private int GrasslandWeight;
        [SerializeField] private int PlainsWeight;
        [SerializeField] private int DesertWeight;
        [SerializeField] private int FloodPlainsWeight;
        [SerializeField] private int TundraWeight;
        [SerializeField] private int SnowWeight;
        [SerializeField] private int ShallowWaterWeight;


        [SerializeField] private int FlatlandWeight;
        [SerializeField] private int HillsWeight;

        [SerializeField] private int NoVegetationWeight;
        [SerializeField] private int ForestWeight;
        [SerializeField] private int JungleWeight;
        [SerializeField] private int MarshWeight;



        public Transform AppearancePrefab {
            get { return _appearancePrefab; }
        }
        [SerializeField] private Transform _appearancePrefab;

        public Sprite Icon {
            get { return _icon; }
        }
        [SerializeField] private Sprite _icon;

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
