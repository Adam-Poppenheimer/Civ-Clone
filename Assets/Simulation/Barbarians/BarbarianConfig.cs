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
        [SerializeField] private float _wanderSelectionWeight_Distance;

        public float WanderSelectionWeight_Allies {
            get { return _wanderSelectionWeight_Allies; }
        }
        [SerializeField] private float _wanderSelectionWeight_Allies;

        public float WanderSelectionWeight_Enemies {
            get { return _wanderSelectionWeight_Enemies; }
        }
        [SerializeField] private float _wanderSelectionWeight_Enemies;

        public int MinEncampmentsPerPlayer {
            get { return _minEncampmentsPerPlayer; }
        }
        [SerializeField] private int _minEncampmentsPerPlayer;

        public int MaxEncampmentsPerPlayer {
            get { return _maxEncampmentsPerPlayer; }
        }
        [SerializeField] private int _maxEncampmentsPerPlayer;

        public float BaseEncampmentSpawnWeight {
            get { return _baseEncampmentSpawnWeight; }
        }
        [SerializeField] private float _baseEncampmentSpawnWeight;

        public float AllyEncampmentSpawnWeight {
            get { return _allyEncampmentSpawnWeight; }
        }
        [SerializeField] private float _allyEncampmentSpawnWeight;

        public float EnemyEncampmentSpawnWeight {
            get { return _enemyEncampmentSpawnWeight; }
        }
        [SerializeField] private float _enemyEncampmentSpawnWeight;

        public int MinEncampmentSpawnProgress {
            get { return _minEncampmentSpawnProgress; }
        }
        [SerializeField] private int _minEncampmentSpawnProgress;

        public int MaxEncampmentSpawnProgress {
            get { return _maxEncampmentSpawnProgress; }
        }
        [SerializeField] private int _maxEncampmentSpawnProgress;

        public int ProgressNeededForUnitSpawn {
            get { return _progressNeededForUnitSpawn; }
        }
        [SerializeField] private int _progressNeededForUnitSpawn;

        public float WaterSpawnChance {
            get { return _waterSpawnChance; }
        }
        [SerializeField, Range(0f, 1f)] private float _waterSpawnChance;

        public IEnumerable<IUnitTemplate> UnitsToSpawn {
            get { return _unitsToSpawn.Cast<IUnitTemplate>(); }
        }
        [SerializeField] private List<UnitTemplate> _unitsToSpawn;

        public float EncampmentBounty {
            get { return _encampmentBounty; }
        }
        [SerializeField] private float _encampmentBounty;




        public int DefendEncampmentRadius {
            get { return _defendEncampmentRadius; }
        }
        [SerializeField] private int _defendEncampmentRadius;

        public float WanderGoalUtility {
            get { return _wanderGoalUtility; }
        }
        [SerializeField, Range(0f, 1f)] private float _wanderGoalUtility;

        public float StayInEncampmentUtility {
            get { return _stayInEncampmentUtility; }
        }
        [SerializeField, Range(0f, 1f)] private float _stayInEncampmentUtility;

        public float HeadTowardsEncampmentUtility {
            get { return _headTowardsEncampmentUtility; }
        }
        [SerializeField, Range(0f, 1f)] private float _headTowardsEncampmentUtility;

        public float PillageUtilityCoefficient {
            get { return _pillageUtilityCoefficient; }
        }
        [SerializeField, Range(0f, 1f)] private float _pillageUtilityCoefficient;

        public float CaptureCivilianUtility {
            get { return _captureCivilianUtility; }
        }
        [SerializeField, Range(0f, 1f)] private float _captureCivilianUtility;

        public float FleeUtilityLogisticSlope {
            get { return _fleeUtilityLogisticSlope; }
        }
        [SerializeField, Range(0f, 0.25f)] private float _fleeUtilityLogisticSlope;

        public float AttackUtilityLogisticsSlope {
            get { return _attackUtilityLogisticsSlope; }
        }
        [SerializeField, Range(0f, 0.25f)] private float _attackUtilityLogisticsSlope;

        public float WaitUntilHealedMaxUtility {
            get { return _waitUntilHealedMaxUtility; }
        }
        [SerializeField, Range(0f, 1f)] private float _waitUntilHealedMaxUtility;

        #endregion

        #endregion
        
    }

}
