using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units {

    [CreateAssetMenu(menuName = "Civ Clone/Unit Config")]
    public class UnitConfig : ScriptableObject, IUnitConfig {

        #region instance fields and properties

        #region from IUnitConfig

        public int MaxHealth {
            get { return _maxHealth; }
        }
        [SerializeField] private int _maxHealth;

        public float RiverCrossingAttackModifier {
            get { return _riverCrossingAttackModifier; }
        }
        [SerializeField] private float _riverCrossingAttackModifier;

        public float CombatBaseDamage {
            get { return _combatBaseDamage; }
        }
        [SerializeField] private float _combatBaseDamage;

        public float TravelSpeedPerSecond {
            get { return _travelSpeedPerSecond; }
        }
        [SerializeField] private float _travelSpeedPerSecond;

        public float RotationSpeedPerSecond {
            get { return _rotationSpeedPerSecond; }
        }
        [SerializeField] private float _rotationSpeedPerSecond;

        public int NextLevelExperienceCoefficient {
            get { return _nextLevelExperienceCoefficient; }
        }
        [SerializeField] private int _nextLevelExperienceCoefficient;

        public int MaxLevel {
            get { return _maxLevel; }
        }
        [SerializeField] private int _maxLevel;

        public int MeleeAttackerExperience {
            get { return _meleeAttackerExperience; }
        }
        [SerializeField] private int _meleeAttackerExperience;

        public int MeleeDefenderExperience {
            get { return _meleeDefenderExperience; }
        }
        [SerializeField] private int _meleeDefenderExperience;

        public int RangedAttackerExperience {
            get { return _rangedAttackerExperience; }
        }
        [SerializeField] private int _rangedAttackerExperience;

        public int RangedDefenderExperience {
            get { return _rangedDefenderExperience; }
        }
        [SerializeField] private int _rangedDefenderExperience;

        public int WoundedThreshold {
            get { return _woundedThreshold; }
        }
        [SerializeField] private int _woundedThreshold;

        public int ForeignLandHealingPerTurn {
            get { return _foreignLandHealingPerTurn; }
        }
        [SerializeField] private int _foreignLandHealingPerTurn;

        public int FriendlyLandHealingPerTurn {
            get { return _friendlyLandHealingPerTurn; }
        }
        [SerializeField] private int _friendlyLandHealingPerTurn;

        public int GarrisonedLandHealingPerTurn {
            get { return _garrisonedLandHealingPerTurn; }
        }
        [SerializeField] private int _garrisonedLandHealingPerTurn;

        public int ForeignNavalHealingPerTurn {
            get { return _foreignNavalHealingPerTurn; }
        }
        [SerializeField] private int _foreignNavalHealingPerTurn;

        public int FriendlyNavalHealingPerTurn {
            get { return _friendlyNavalHealingPerTurn; }
        }
        [SerializeField] private int _friendlyNavalHealingPerTurn;

        public int GarrisonedNavalHealingPerTurn {
            get { return _garrisonedNavalHealingPerTurn; }
        }
        [SerializeField] private int _garrisonedNavalHealingPerTurn;

        public float CityRepairPercentPerTurn {
            get { return _cityRepairPercentPerTurn; }
        }
        [SerializeField] private int _cityRepairPercentPerTurn;

        #endregion

        [SerializeField] private List<float> TerrainDefensiveness;        
        [SerializeField] private List<float> ShapeDefensiveness;
        [SerializeField] private List<float> VegetationDefensiveness;

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

        #endregion

        #endregion

    }

}
