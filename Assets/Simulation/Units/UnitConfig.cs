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

        public ReadOnlyCollection<float> TerrainMeleeDefensiveness {
            get { return _terrainMeleeDefensiveness.AsReadOnly(); }
        }
        [SerializeField] private List<float> _terrainMeleeDefensiveness;

        public ReadOnlyCollection<float> TerrainRangedDefensiveness {
            get { return _terrainRangedDefensiveness.AsReadOnly(); }
        }
        [SerializeField] private List<float> _terrainRangedDefensiveness;

        public ReadOnlyCollection<float> FeatureMeleeDefensiveness {
            get { return _featureMeleeDefensiveness.AsReadOnly(); }
        }
        [SerializeField] private List<float> _featureMeleeDefensiveness;

        public ReadOnlyCollection<float> FeatureRangedDefensiveness {
            get { return _featureRangedDefensiveness.AsReadOnly(); }
        }
        [SerializeField] private List<float> _featureRangedDefensiveness;

        public float RiverCrossingAttackModifier {
            get { return _riverCrossingAttackModifier; }
        }
        [SerializeField] private float _riverCrossingAttackModifier;

        public float CombatBaseDamage {
            get { return _combatBaseDamage; }
        }
        [SerializeField] private float _combatBaseDamage;

        public int VisionRange {
            get { return _visionRange; }
        }
        [SerializeField] private int _visionRange;

        #endregion

        #endregion
        
    }

}
