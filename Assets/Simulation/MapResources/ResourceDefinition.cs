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


        public bool ValidOnGrassland    { get { return GrasslandWeight    > 0; } }
        public bool ValidOnPlains       { get { return PlainsWeight       > 0; } }
        public bool ValidOnDesert       { get { return DesertWeight       > 0; } }
        public bool ValidOnTundra       { get { return TundraWeight       > 0; } }
        public bool ValidOnSnow         { get { return SnowWeight         > 0; } }
        public bool ValidOnShallowWater { get { return ShallowWaterWeight > 0; } }
        
        public bool ValidOnHills        { get { return HillWeight         > 0; } }

        public bool ValidOnForest       { get { return ForestWeight       > 0; } }
        public bool ValidOnJungle       { get { return JungleWeight       > 0; } }
        public bool ValidOnMarsh        { get { return MarshWeight        > 0; } }


        public int GrasslandWeight {
            get { return _grasslandWeight; }
        }
        [SerializeField] private int _grasslandWeight;

        public int PlainsWeight {
            get { return _plainsWeight; }
        }
        [SerializeField] private int _plainsWeight;

        public int DesertWeight {
            get { return _desertWeight; }
        }
        [SerializeField] private int _desertWeight;

        public int TundraWeight {
            get { return _tundraWeight; }
        }
        [SerializeField] private int _tundraWeight;

        public int SnowWeight {
            get { return _snowWeight; }
        }
        [SerializeField] private int _snowWeight;

        public int ShallowWaterWeight {
            get { return _shallowWaterWeight; }
        }
        [SerializeField] private int _shallowWaterWeight;


        public int HillWeight {
            get { return _hillWeight; }
        }
        [SerializeField] private int _hillWeight;


        public int ForestWeight {
            get { return _forestWeight; }
        }
        [SerializeField] private int _forestWeight;

        public int JungleWeight {
            get { return _jungleWeight; }
        }
        [SerializeField] private int _jungleWeight;

        public int MarshWeight {
            get { return _marshWeight; }
        }
        [SerializeField] private int _marshWeight;



        public int SelectionWeight {
            get { return _selectionWeight; }
        }
        [SerializeField] private int _selectionWeight;

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
        
    }

}
