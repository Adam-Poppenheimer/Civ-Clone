using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Cities {

    [CreateAssetMenu(menuName = "Civ Clone/Population Growth Config")]
    public class PopulationGrowthConfig : ScriptableObject, IPopulationGrowthConfig {

        #region instance fields and properties

        public int FoodConsumptionPerPerson {
            get { return _foodConsumptionPerPerson; }
        }
        [SerializeField] private int _foodConsumptionPerPerson;

        public int BaseStockpile {
            get { return _baseStockpile; }
        }
        [SerializeField] private int _baseStockpile;

        public int PreviousPopulationCoefficient {
            get { return _previousPopulationCoefficient; }
        }
        [SerializeField] private int _previousPopulationCoefficient;

        public float PreviousPopulationExponent {
            get { return _previousPopulationExponent; }
        }
        [SerializeField] private float _previousPopulationExponent;

        #endregion

    }

}
