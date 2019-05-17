using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Zenject;
using TMPro;

using Assets.Simulation;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;

using Assets.Simulation.Units;

namespace Assets.UI.Cities.Production {

    public class CityProjectRecord : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

        #region instance fields and properties

        public IUnitTemplate UnitTemplate { get; set; }

        public IBuildingTemplate BuildingTemplate { get; set; }

        public Button SelectionButton {
            get { return _selectionButton; }
        }
        [SerializeField] private Button _selectionButton = null;

        [SerializeField] private Text            NameField = null;
        [SerializeField] private TextMeshProUGUI CostField = null;



        private IYieldFormatter    YieldFormatter;
        private DescriptionTooltip DescriptionTooltip;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IYieldFormatter yieldFormatter, DescriptionTooltip descriptionTooltip
        ) {
            YieldFormatter     = yieldFormatter;
            DescriptionTooltip = descriptionTooltip;
        }

        #region EventSystem handlers

        public void OnPointerEnter(PointerEventData eventData) {
            if(UnitTemplate != null) {
                DescriptionTooltip.SetDescriptionFrom(UnitTemplate);

            }else if(BuildingTemplate != null) {
                DescriptionTooltip.SetDescriptionFrom(BuildingTemplate);
            }

            DescriptionTooltip.gameObject.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData) {
            DescriptionTooltip.gameObject.SetActive(false);
        }

        #endregion

        public void Refresh() {
            if(UnitTemplate != null) {
                NameField.text = UnitTemplate.name;
                CostField.text = YieldFormatter.GetTMProFormattedSingleResourceString(
                    YieldType.Production, UnitTemplate.ProductionCost
                );

            }else if(BuildingTemplate != null) {
                NameField.text = BuildingTemplate.name;
                CostField.text = YieldFormatter.GetTMProFormattedSingleResourceString(
                    YieldType.Production, BuildingTemplate.ProductionCost
                );
            }
        }

        #endregion

    }

}
