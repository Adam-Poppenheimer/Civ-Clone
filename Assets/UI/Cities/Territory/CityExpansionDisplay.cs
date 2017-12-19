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

namespace Assets.UI.Cities.Territory {

    public class CityExpansionDisplay : CityDisplayBase {

        #region instance fields and properties

        [InjectOptional(Id = "Culture Stockpile Field")] private Text CultureStockpileField {
            get { return _cultureStockpileField; }
            set {
                if(value != null) {
                    _cultureStockpileField = value;
                }
            }
        }
        [SerializeField] private Text _cultureStockpileField;

        [InjectOptional(Id = "Culture Needed Field")] private Text CultureNeededField {
            get { return _cultureNeededField; }
            set {
                if(value != null) {
                    _cultureNeededField = value;
                }
            }
        }
        [SerializeField] private Text _cultureNeededField;

        [InjectOptional(Id = "Turns Until Expansion Field")] private Text TurnsUntilExpansionField {
            get { return _turnsUntilExpansionField; }
            set {
                if(value != null) {
                    _turnsUntilExpansionField = value;
                }
            }
        }
        [SerializeField] private Text _turnsUntilExpansionField;

        [InjectOptional(Id = "Expansion Progress Slider")] private Slider ExpansionProgressSlider {
            get { return _expansionProgressSlider; }
            set {
                if(value != null) {
                    _expansionProgressSlider = value;
                }
            }
        }
        [SerializeField] private Slider _expansionProgressSlider;

        [InjectOptional(Id = "Next Tile Indicator")] private RectTransform NextTileIndicator {
            get { return _nextTileIndicator; }
            set {
                if(value != null) {
                    _nextTileIndicator = value;
                }
            }
        }
        [SerializeField] private RectTransform _nextTileIndicator;

        private IBorderExpansionLogic ExpansionLogic;
        private IResourceGenerationLogic ResourceGenerationLogic;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(IBorderExpansionLogic expansionLogic, IResourceGenerationLogic resourceGenerationLogic) {
            ExpansionLogic = expansionLogic;
            ResourceGenerationLogic = resourceGenerationLogic;
        }

        #region Unity message methods

        private void Update() {
            Refresh();
        }

        #endregion

        #region from CityDisplayBase

        public override void Refresh() {
            if(ObjectToDisplay == null) {
                return;
            }

            var nextTile = ExpansionLogic.GetNextTileToPursue(ObjectToDisplay);

            var currentCulture = ObjectToDisplay.CultureStockpile;
            var neededCulture = ExpansionLogic.GetCultureCostOfAcquiringTile(ObjectToDisplay, nextTile);

            var culturePerTurn = ResourceGenerationLogic.GetTotalYieldForCity(ObjectToDisplay)[ResourceType.Culture];

            CultureStockpileField.text = currentCulture.ToString();
            CultureNeededField.text = neededCulture.ToString();

            if(culturePerTurn == 0) {
                TurnsUntilExpansionField.text = "--";
            }else {
                TurnsUntilExpansionField.text = Mathf.CeilToInt((neededCulture - currentCulture) / (float)culturePerTurn).ToString();
            }            

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
