using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Territory;
using Assets.Simulation.Cities.ResourceGeneration;

namespace Assets.UI.Cities.TilePossession {

    public class CityExpansionDisplay : CityDisplayBase {

        #region instance fields and properties

        [SerializeField] private Text CultureStockpileField;
        [SerializeField] private Text CultureNeededField;

        [SerializeField] private Text TurnsUntilExpansionField;

        [SerializeField] private Slider ExpansionProgressSlider;

        [SerializeField] private RectTransform NextTileIndicator;

        private IBorderExpansionLogic ExpansionLogic;
        private IResourceGenerationLogic ResourceGenerationLogic;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(IBorderExpansionLogic expansionLogic, IResourceGenerationLogic resourceGenerationLogic) {
            ExpansionLogic = expansionLogic;
            ResourceGenerationLogic = resourceGenerationLogic;
        }

        #region from CityDisplayBase

        protected override void DisplayCity(ICity city) {
            var nextTile = ExpansionLogic.GetNextTileToPursue(city);

            var currentCulture = city.CultureStockpile;
            var neededCulture = ExpansionLogic.GetCultureCostOfAcquiringTile(city, nextTile);

            var culturePerTurn = ResourceGenerationLogic.GetTotalYieldForCity(city)[ResourceType.Culture];

            CultureStockpileField.text = currentCulture.ToString();
            CultureNeededField.text = neededCulture.ToString();

            TurnsUntilExpansionField.text = Mathf.CeilToInt((neededCulture - currentCulture) / (float)culturePerTurn).ToString();

            ExpansionProgressSlider.minValue = 0;
            ExpansionProgressSlider.maxValue = neededCulture;
            ExpansionProgressSlider.value = currentCulture;

            NextTileIndicator.gameObject.SetActive(true);
            NextTileIndicator.position = Camera.main.WorldToScreenPoint(nextTile.transform.position);          
        }

        #endregion

        #endregion
        

        
    }

}
