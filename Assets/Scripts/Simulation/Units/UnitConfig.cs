using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units {

    [CreateAssetMenu(menuName = "Civ Clone/Units/Config")]
    public class UnitConfig : ScriptableObject, IUnitConfig {

        #region instance fields and properties

        #region from IUnitConfig

        public int MaxHealth {
            get { return _maxHealth; }
        }
        [SerializeField] private int _maxHealth = 0;

        public float RiverCrossingAttackModifier {
            get { return _riverCrossingAttackModifier; }
        }
        [SerializeField] private float _riverCrossingAttackModifier = 0f;

        public float CombatBaseDamage {
            get { return _combatBaseDamage; }
        }
        [SerializeField] private float _combatBaseDamage = 0f;

        public float TravelSpeedPerSecond {
            get { return _travelSpeedPerSecond; }
        }
        [SerializeField] private float _travelSpeedPerSecond = 0f;

        public float RotationSpeedPerSecond {
            get { return _rotationSpeedPerSecond; }
        }
        [SerializeField] private float _rotationSpeedPerSecond = 0f;

        public int NextLevelExperienceCoefficient {
            get { return _nextLevelExperienceCoefficient; }
        }
        [SerializeField] private int _nextLevelExperienceCoefficient = 0;

        public int MaxLevel {
            get { return _maxLevel; }
        }
        [SerializeField] private int _maxLevel = 0;

        public int MeleeAttackerExperience {
            get { return _meleeAttackerExperience; }
        }
        [SerializeField] private int _meleeAttackerExperience = 0;

        public int MeleeDefenderExperience {
            get { return _meleeDefenderExperience; }
        }
        [SerializeField] private int _meleeDefenderExperience = 0;

        public int RangedAttackerExperience {
            get { return _rangedAttackerExperience; }
        }
        [SerializeField] private int _rangedAttackerExperience = 0;

        public int RangedDefenderExperience {
            get { return _rangedDefenderExperience; }
        }
        [SerializeField] private int _rangedDefenderExperience = 0;

        public int WoundedThreshold {
            get { return _woundedThreshold; }
        }
        [SerializeField] private int _woundedThreshold = 0;

        public int ForeignLandHealingPerTurn {
            get { return _foreignLandHealingPerTurn; }
        }
        [SerializeField] private int _foreignLandHealingPerTurn = 0;

        public int FriendlyLandHealingPerTurn {
            get { return _friendlyLandHealingPerTurn; }
        }
        [SerializeField] private int _friendlyLandHealingPerTurn = 0;

        public int GarrisonedLandHealingPerTurn {
            get { return _garrisonedLandHealingPerTurn; }
        }
        [SerializeField] private int _garrisonedLandHealingPerTurn = 0;

        public int ForeignNavalHealingPerTurn {
            get { return _foreignNavalHealingPerTurn; }
        }
        [SerializeField] private int _foreignNavalHealingPerTurn = 0;

        public int FriendlyNavalHealingPerTurn {
            get { return _friendlyNavalHealingPerTurn; }
        }
        [SerializeField] private int _friendlyNavalHealingPerTurn = 0;

        public int GarrisonedNavalHealingPerTurn {
            get { return _garrisonedNavalHealingPerTurn; }
        }
        [SerializeField] private int _garrisonedNavalHealingPerTurn = 0;

        public float CityRepairPercentPerTurn {
            get { return _cityRepairPercentPerTurn; }
        }
        [SerializeField] private int _cityRepairPercentPerTurn = 0;

        public float FortificationBonusPerTurn {
            get { return _fortificationBonusPerTurn; }
        }
        [SerializeField] private float _fortificationBonusPerTurn = 0f;

        public float MaxFortificationBonus {
            get { return _maxFortificationBonus; }
        }
        [SerializeField] private float _maxFortificationBonus = 0f;

        public GameObject UnitPrefab {
            get { return _unitPrefab; }
        }
        [SerializeField] private GameObject _unitPrefab = null;

        public IEnumerable<GreatPersonType> GreatPeopleCivilianTypes {
            get { return _greatPeopleCivilianTypes; }
        }
        [SerializeField] private List<GreatPersonType> _greatPeopleCivilianTypes = null;

        public IEnumerable<GreatPersonType> GreatPeopleMilitaryTypes {
            get { return _greatPeopleMilitaryTypes; }
        }
        [SerializeField] private List<GreatPersonType> _greatPeopleMilitaryTypes = null;

        public int AuraRange {
            get { return _auraRange; }
        }
        [SerializeField] private int _auraRange = 0;

        public IEnumerable<IUnitTemplate> CapturableTemplates {
            get { return _capturableTemplates.Cast<IUnitTemplate>(); }
        }
        [SerializeField] private List<UnitTemplate> _capturableTemplates = null;

        #endregion

        [SerializeField] private List<float> TerrainDefensiveness    = null;        
        [SerializeField] private List<float> ShapeDefensiveness      = null;
        [SerializeField] private List<float> VegetationDefensiveness = null;

        [SerializeField] private UnitTemplate GreatAdmiralTemplate   = null;
        [SerializeField] private UnitTemplate GreatAristTemplate     = null;
        [SerializeField] private UnitTemplate GreatEngineerTemplate  = null;
        [SerializeField] private UnitTemplate GreatGeneralTemplate   = null;
        [SerializeField] private UnitTemplate GreatMerchantTemplate  = null;
        [SerializeField] private UnitTemplate GreatScientistTemplate = null;

        #endregion

        #region instance methods

        #region from IUnitConfig

        public float GetTerrainDefensiveness(CellTerrain terrain) {
            var index = (int)terrain;

            if(index >= TerrainDefensiveness.Count) {
                return 0;
            }else {
                return TerrainDefensiveness[index];
            }
        }

        public float GetShapeDefensiveness(CellShape shape) {
            var index = (int)shape;

            if(index >= ShapeDefensiveness.Count) {
                return 0;
            }else {
                return ShapeDefensiveness[index];
            }
        }

        public float GetVegetationDefensiveness(CellVegetation vegetation) {
            var index = (int)vegetation;

            if(index >= VegetationDefensiveness.Count) {
                return 0;
            }else {
                return VegetationDefensiveness[index];
            }
        }

        public IUnitTemplate GetTemplateForGreatPersonType(GreatPersonType type) {
            switch(type) {
                case GreatPersonType.GreatAdmiral:   return GreatAdmiralTemplate;
                case GreatPersonType.GreatArtist:    return GreatAristTemplate;
                case GreatPersonType.GreatEngineer:  return GreatEngineerTemplate;
                case GreatPersonType.GreatGeneral:   return GreatGeneralTemplate;
                case GreatPersonType.GreatMerchant:  return GreatMerchantTemplate;
                case GreatPersonType.GreatScientist: return GreatScientistTemplate;
                default: throw new NotImplementedException("No template for great person type " + type);
            }
        }

        #endregion

        #endregion

    }

}
