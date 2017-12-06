using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.Cities {

    [CreateAssetMenu(menuName = "Civ Clone/City Config")]
    public class CityConfig : ScriptableObject, ICityConfig {

        #region instance fields and properties

        #region from ICityConfig

        public int BaseGrowthStockpile {
            get { return _baseGrowthStockpile; }
        }
        [SerializeField] private int _baseGrowthStockpile;

        public int FoodConsumptionPerPerson {
            get { return _foodConsumptionPerPerson; }
        }
        [SerializeField] private int _foodConsumptionPerPerson;

        public int GrowthPreviousPopulationCoefficient {
            get { return _growthPreviousPopulationCoefficient; }
        }
        [SerializeField] private int _growthPreviousPopulationCoefficient;

        public float GrowthPreviousPopulationExponent {
            get { return _growthPreviousPopulationExponent; }
        }
        [SerializeField] private float _growthPreviousPopulationExponent;

        public float HurryCostPerProduction {
            get { return _hurryCostPerProduction; }
        }
        [SerializeField] private float _hurryCostPerProduction;

        public int MaxBorderRange {
            get { return _maxBorderRange; }
        }
        [SerializeField] private int _maxBorderRange;

        public int PreviousTileCountCoefficient {
            get { return _previousTileCountCoefficient; }
        }
        [SerializeField] private int _previousTileCountCoefficient;

        public float PreviousTileCountExponent {
            get { return _previousTileCountExponent; }
        }
        [SerializeField] private float _previousTileCountExponent;

        public int TileCostBase {
            get { return _tileCostBase; }
        }
        [SerializeField] private int _tileCostBase;

        public ResourceSummary UnemployedYield {
            get { return _unemployedYield; }
        }
        [SerializeField] private ResourceSummary _unemployedYield;

        public int MinimumSeparation {
            get { return _minimumSeparation; }
        }
        [SerializeField] private int _minimumSeparation;

        #endregion

        #endregion
        
    }

}
