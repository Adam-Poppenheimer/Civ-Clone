using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.Units {

    [CreateAssetMenu(menuName = "Civ Clone/Unit Config")]
    public class UnitConfig : ScriptableObject, IUnitConfig {

        #region instance fields and properties

        #region from IUnitConfig

        public int MaxHealth {
            get { return _maxHealth; }
        }
        [SerializeField] private int _maxHealth;

        public ReadOnlyCollection<float> TerrainDefensiveness {
            get { return _terrainDefensiveness.AsReadOnly(); }
        }
        [SerializeField] private List<float> _terrainDefensiveness;

        public ReadOnlyCollection<float> FeatureDefensiveness {
            get { return _featureDefensiveness.AsReadOnly(); }
        }
        [SerializeField] private List<float> _featureDefensiveness;

        public ReadOnlyCollection<float> ShapeDefensiveness {
            get { return _shapeDefensiveness.AsReadOnly(); }
        }
        [SerializeField] private List<float> _shapeDefensiveness;

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

        #endregion
        
    }

}
