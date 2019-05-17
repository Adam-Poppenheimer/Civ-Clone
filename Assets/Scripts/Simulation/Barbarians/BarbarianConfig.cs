using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.Units;

namespace Assets.Simulation.Barbarians {

    [CreateAssetMenu(menuName = "Civ Clone/Barbarians/Config")]
    public class BarbarianConfig : ScriptableObject, IBarbarianConfig {

        #region instance fields and properties

        #region from IBarbarianConfig        

        public float WanderSelectionWeight_Distance {
            get { return _wanderSelectionWeight_Distance; }
        }
        [SerializeField] private float _wanderSelectionWeight_Distance = 0f;

        public float WanderSelectionWeight_Allies {
            get { return _wanderSelectionWeight_Allies; }
        }
        [SerializeField] private float _wanderSelectionWeight_Allies = 0f;

        public float WanderSelectionWeight_Enemies {
            get { return _wanderSelectionWeight_Enemies; }
        }
        [SerializeField] private float _wanderSelectionWeight_Enemies = 0f;

        public int MinEncampmentsPerPlayer {
            get { return _minEncampmentsPerPlayer; }
        }
        [SerializeField] private int _minEncampmentsPerPlayer = 0;

        public int MaxEncampmentsPerPlayer {
            get { return _maxEncampmentsPerPlayer; }
        }
        [SerializeField] private int _maxEncampmentsPerPlayer = 0;

        public float BaseEncampmentSpawnWeight {
            get { return _baseEncampmentSpawnWeight; }
        }
        [SerializeField] private float _baseEncampmentSpawnWeight = 0f;

        public float AllyEncampmentSpawnWeight {
            get { return _allyEncampmentSpawnWeight; }
        }
        [SerializeField] private float _allyEncampmentSpawnWeight = 0f;

        public float EnemyEncampmentSpawnWeight {
            get { return _enemyEncampmentSpawnWeight; }
        }
        [SerializeField] private float _enemyEncampmentSpawnWeight = 0f;

        public int MinEncampmentSpawnProgress {
            get { return _minEncampmentSpawnProgress; }
        }
        [SerializeField] private int _minEncampmentSpawnProgress = 0;

        public int MaxEncampmentSpawnProgress {
            get { return _maxEncampmentSpawnProgress; }
        }
        [SerializeField] private int _maxEncampmentSpawnProgress = 0;

        public int ProgressNeededForUnitSpawn {
            get { return _progressNeededForUnitSpawn; }
        }
        [SerializeField] private int _progressNeededForUnitSpawn = 0;

        public float WaterSpawnChance {
            get { return _waterSpawnChance; }
        }
        [SerializeField, Range(0f, 1f)] private float _waterSpawnChance = 0f;

        public IEnumerable<IUnitTemplate> UnitsToSpawn {
            get { return _unitsToSpawn.Cast<IUnitTemplate>(); }
        }
        [SerializeField] private List<UnitTemplate> _unitsToSpawn = null;

        public float EncampmentBounty {
            get { return _encampmentBounty; }
        }
        [SerializeField] private float _encampmentBounty = 0f;




        public int DefendEncampmentRadius {
            get { return _defendEncampmentRadius; }
        }
        [SerializeField] private int _defendEncampmentRadius = 0;

        public float WanderGoalUtility {
            get { return _wanderGoalUtility; }
        }
        [SerializeField, Range(0f, 1f)] private float _wanderGoalUtility = 0f;

        public float StayInEncampmentUtility {
            get { return _stayInEncampmentUtility; }
        }
        [SerializeField, Range(0f, 1f)] private float _stayInEncampmentUtility = 0f;

        public float HeadTowardsEncampmentUtility {
            get { return _headTowardsEncampmentUtility; }
        }
        [SerializeField, Range(0f, 1f)] private float _headTowardsEncampmentUtility = 0f;

        public float PillageUtilityCoefficient {
            get { return _pillageUtilityCoefficient; }
        }
        [SerializeField, Range(0f, 1f)] private float _pillageUtilityCoefficient = 0f;

        public float CaptureCivilianUtility {
            get { return _captureCivilianUtility; }
        }
        [SerializeField, Range(0f, 1f)] private float _captureCivilianUtility = 0f;

        public float FleeUtilityLogisticSlope {
            get { return _fleeUtilityLogisticSlope; }
        }
        [SerializeField, Range(0f, 0.25f)] private float _fleeUtilityLogisticSlope = 0f;

        public float AttackUtilityLogisticsSlope {
            get { return _attackUtilityLogisticsSlope; }
        }
        [SerializeField, Range(0f, 0.25f)] private float _attackUtilityLogisticsSlope = 0f;

        public float WaitUntilHealedMaxUtility {
            get { return _waitUntilHealedMaxUtility; }
        }
        [SerializeField, Range(0f, 1f)] private float _waitUntilHealedMaxUtility = 0f;

        #endregion

        #endregion
        
    }

}
