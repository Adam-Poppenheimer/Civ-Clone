﻿using System;
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

        #endregion

        [SerializeField] private List<float> TerrainDefensiveness;        
        [SerializeField] private List<float> ShapeDefensiveness;
        [SerializeField] private List<float> FeatureDefensiveness;

        #endregion

        #region instance methods

        #region from IUnitConfig

        public float GetTerrainDefensiveness(TerrainType terrain) {
            var index = (int)terrain;

            if(index >= TerrainDefensiveness.Count) {
                return 0;
            }else {
                return TerrainDefensiveness[index];
            }
        }

        public float GetShapeDefensiveness(TerrainShape shape) {
            var index = (int)shape;

            if(index >= ShapeDefensiveness.Count) {
                return 0;
            }else {
                return ShapeDefensiveness[index];
            }
        }

        public float GetFeatureDefensiveness(TerrainFeature feature) {
            var index = (int)feature;

            if(index >= FeatureDefensiveness.Count) {
                return 0;
            }else {
                return FeatureDefensiveness[index];
            }
        }

        #endregion

        #endregion

    }

}
