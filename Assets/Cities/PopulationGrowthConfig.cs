﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Cities {

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
            get { return _currentPopulationCoefficient; }
        }
        [SerializeField] private int _currentPopulationCoefficient;

        public float PreviousPopulationExponent {
            get { return _currentPopulationExponent; }
        }
        [SerializeField] private float _currentPopulationExponent;

        #endregion

    }

}
