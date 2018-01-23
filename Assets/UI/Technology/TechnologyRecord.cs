using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation.Technology;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;
using Assets.Simulation.Cities.Buildings;

namespace Assets.UI.Technology {

    public class TechnologyRecord : MonoBehaviour {

        #region internal types

        public enum TechStatus {
            Available, Discovered, BeingResearched, InQueue
        }

        #endregion

        #region instance fields and properties

        public Vector2 BackwardConnectionPoint {
            get {
                return new Vector2(
                    RectTransform.rect.x,
                    RectTransform.rect.y + RectTransform.rect.height / 2f
                ) + (Vector2)RectTransform.localPosition;
            }
        }

        public Vector2 ForwardConnectionPoint {
            get {
                return new Vector2(
                    RectTransform.rect.x + RectTransform.rect.width,
                    RectTransform.rect.y + RectTransform.rect.height / 2f
                ) + (Vector2)RectTransform.localPosition;
            }
        }

        public ITechDefinition TechToDisplay { get; set; }

        public TechStatus Status { get; set; }

        public int TurnsToResearch { get; set; }

        public int CurrentProgress { get; set; }

        public Button SelectionButton {
            get {
                if(_selectionButton == null) {
                    _selectionButton = GetComponentInChildren<Button>();
                }
                return _selectionButton;
            }
        }
        private Button _selectionButton;

        private RectTransform RectTransform {
            get {
                if(_rectTransform == null) {
                    _rectTransform = GetComponent<RectTransform>();
                }
                return _rectTransform;
            }
        }
        private RectTransform _rectTransform;

        [SerializeField] private Text   NameField;
        [SerializeField] private Text   CostField;
        [SerializeField] private Text   TurnsToResearchField;
        [SerializeField] private Slider ProgressSlider;

        [SerializeField] private Color DiscoveredColor;
        [SerializeField] private Color AvailableColor;
        [SerializeField] private Color BeingResearchedColor;
        [SerializeField] private Color InQueueColor;

        #endregion

        #region instance methods

        public void Refresh() {
            ClearDisplay();
            if(TechToDisplay == null) {
                return;
            }

            if(NameField != null) {
                NameField.text = TechToDisplay.Name;
            }
            
            if(CostField != null) {
                CostField.text = TechToDisplay.Cost.ToString();
            }
            
            if(TurnsToResearchField != null) {
                if(Status == TechStatus.Discovered) {
                    TurnsToResearchField.gameObject.SetActive(false);
                }else {
                    TurnsToResearchField.gameObject.SetActive(true);
                    TurnsToResearchField.text = string.Format("{0} Turns", TurnsToResearch);
                }
            }

            if(ProgressSlider != null) {
                ProgressSlider.minValue = 0;
                ProgressSlider.maxValue = TechToDisplay.Cost;
                ProgressSlider.value    = CurrentProgress;
            }

            if(SelectionButton != null) {
                if(Status == TechStatus.Discovered) {
                    SelectionButton.image.color = DiscoveredColor;
                    SelectionButton.interactable = false;

                }else if(Status == TechStatus.Available){
                    SelectionButton.image.color = AvailableColor;
                    SelectionButton.interactable = true;

                }else if(Status == TechStatus.BeingResearched) {
                    SelectionButton.image.color = BeingResearchedColor;
                    SelectionButton.interactable = true;

                }else if(Status == TechStatus.InQueue) {
                    SelectionButton.image.color = InQueueColor;
                    SelectionButton.interactable = true;
                }
            }
        }

        private void ClearDisplay() {
            if(NameField != null) {
                NameField.text = "--";
            }

            if(CostField != null) {
                CostField.text = "--";
            }

            if(TurnsToResearchField != null) {
                TurnsToResearchField.text = "--";
            }

            if(ProgressSlider != null) {
                ProgressSlider.minValue = 0;
                ProgressSlider.maxValue = 0;
                ProgressSlider.value = 0;
            }

            if(SelectionButton != null) {
                SelectionButton.interactable = false;
            }
        }

        #endregion

    }

}
