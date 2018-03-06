using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.Civilizations {

    /// <summary>
    /// The standard implementation of ICivilizationConfig
    /// </summary>
    [CreateAssetMenu(menuName = "Civ Clone/Civilization Config")]
    public class CivilizationConfig : ScriptableObject, ICivilizationConfig {

        #region instance fields and properties

        #region from ICivilizationConfig

        public int BaseHappiness {
            get { return _baseHappiness; }
        }
        [SerializeField] private int _baseHappiness;

        public int HappinessPerLuxury {
            get { return _happinessPerLuxury; }
        }
        [SerializeField] private int _happinessPerLuxury;

        public float YieldLossPerUnhappiness {
            get { return _yieldLossPerUnhappiness; }
        }
        [SerializeField] private float _yieldLossPerUnhappiness;

        public float ModifierLossPerUnhappiness {
            get { return _modifierLossPerUnhappiness; }
        }
        [SerializeField] private float _modifierLossPerUnhappiness;

        #endregion

        #endregion

    }

}
