using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

namespace Assets.Cities.UI {

    public class CityExpansionDisplay : MonoBehaviour, ICityExpansionDisplay {

        #region instance fields and properties

        #region from ICityExpansionDisplay

        public ICity CityToDisplay { get; set; }

        #endregion

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

        #region from ICityExpansionDisplay

        public void Refresh() {
            if(CityToDisplay != null) {
                var nextTile = ExpansionLogic.GetNextTileToPursue(CityToDisplay);

                var currentCulture = CityToDisplay.CultureStockpile;
                var neededCulture = ExpansionLogic.GetCultureCostOfAcquiringTile(CityToDisplay, nextTile);

                var culturePerTurn = ResourceGenerationLogic.GetTotalYieldForCity(CityToDisplay)[ResourceType.Culture];

                CultureStockpileField.text = currentCulture.ToString();
                CultureNeededField.text = neededCulture.ToString();

                TurnsUntilExpansionField.text = Mathf.CeilToInt((neededCulture - currentCulture) / (float)culturePerTurn).ToString();

                ExpansionProgressSlider.minValue = 0;
                ExpansionProgressSlider.maxValue = neededCulture;
                ExpansionProgressSlider.value = currentCulture;

                NextTileIndicator.gameObject.SetActive(true);
                NextTileIndicator.position = Camera.main.WorldToScreenPoint(nextTile.transform.position);
            }            
        }

        #endregion

        #endregion
        

        
    }

}
