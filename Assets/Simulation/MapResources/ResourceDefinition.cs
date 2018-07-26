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




        public bool ValidOnGrassland {
            get { return _validOnGrassland; }
        }
        [SerializeField] private bool _validOnGrassland;

        public bool ValidOnPlains {
            get { return _validOnPlains; }
        }
        [SerializeField] private bool _validOnPlains;

        public bool ValidOnDesert {
            get { return _validOnDesert; }
        }
        [SerializeField] private bool _validOnDesert;

        public bool ValidOnTundra {
            get { return _validOnTundra; }
        }
        [SerializeField] private bool _validOnTundra;

        public bool ValidOnSnow {
            get { return _validOnSnow; }
        }
        [SerializeField] private bool _validOnSnow;

        public bool ValidOnShallowWater {
            get { return _validOnCoast; }
        }
        [SerializeField] private bool _validOnCoast;


        public bool ValidOnHills {
            get { return _validOnHills; }
        }
        [SerializeField] private bool _validOnHills;


        public bool ValidOnForest {
            get { return _validOnForest; }
        }
        [SerializeField] private bool _validOnForest;

        public bool ValidOnJungle {
            get { return _validOnJungle; }
        }
        [SerializeField] private bool _validOnJungle;

        public bool ValidOnMarsh {
            get { return _validOnMarsh; }
        }
        [SerializeField] private bool _validOnMarsh;




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
