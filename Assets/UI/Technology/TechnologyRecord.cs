using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation.Technology;
using Assets.Simulation.Core;
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

        #region static fields and properties

        private static string DoesNotRequireFreshWaterFormat =  "{0} <color #000000>for all {1} improvements with access to fresh water";
        private static string RequiresFreshWaterFormat       =  "{0} <color #000000>for all {1} improvements";

        #endregion

        #region instance fields and properties

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
        [SerializeField] private Image  IconField;

        [SerializeField] private RectTransform        BoonRecordContainer;
        [SerializeField] private TechnologyBoonRecord BoonRecordPrefab;

        [SerializeField] private Color DiscoveredColor;
        [SerializeField] private Color AvailableColor;
        [SerializeField] private Color BeingResearchedColor;
        [SerializeField] private Color InQueueColor;

        private List<TechnologyBoonRecord> InstantiatedBoonRecords = 
            new List<TechnologyBoonRecord>();



        private ICoreConfig CoreConfig;

        private IYieldFormatter YieldFormatter;

        private DiContainer Container;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            ICoreConfig coreConfig, IYieldFormatter yieldFormatter, DiContainer container
        ) {
            CoreConfig     = coreConfig;
            YieldFormatter = yieldFormatter;
            Container      = container;
        }

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

            if(IconField != null) {
                IconField.sprite = TechToDisplay.Icon;
            }

            if(BoonRecordContainer != null) {
                PopulateBoons();
            }
        }

        private void PopulateBoons() {
            foreach(var ability in TechToDisplay.AbilitiesEnabled) {
                BuildBoonRecord(ability.Icon, ability.Description);
            }

            foreach(var unit in TechToDisplay.UnitsEnabled) {
                BuildBoonRecord(unit.Icon, unit.Description);
            }

            foreach(var building in TechToDisplay.BuildingsEnabled) {
                BuildBoonRecord(building.Icon, building.Description);
            }

            foreach(var yieldModification in TechToDisplay.ImprovementYieldModifications) {
                var description = String.Format(
                    yieldModification.RequiresFreshWater ? RequiresFreshWaterFormat : DoesNotRequireFreshWaterFormat,
                    YieldFormatter.GetTMProFormattedYieldString(yieldModification.BonusYield, false),
                    yieldModification.Template.name
                );

                BuildBoonRecord(CoreConfig.YieldModificationIcon, description);
            }
        }

        private TechnologyBoonRecord BuildBoonRecord(Sprite icon, string description) {
            var newRecord = Container.InstantiatePrefabForComponent<TechnologyBoonRecord>(BoonRecordPrefab);

            newRecord.Icon = icon;
            newRecord.Description = description;

            newRecord.transform.SetParent(BoonRecordContainer, false);
            newRecord.gameObject.SetActive(true);

            InstantiatedBoonRecords.Add(newRecord);

            return newRecord;
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

            if(IconField != null) {
                IconField.sprite = null;
            }

            foreach(var boonRecord in new List<TechnologyBoonRecord>(InstantiatedBoonRecords)) {
                Destroy(boonRecord.gameObject);
            }

            InstantiatedBoonRecords.Clear();
        }

        #endregion

    }

}
