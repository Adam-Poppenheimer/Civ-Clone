using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;
using TMPro;

using Assets.Simulation;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;

using Assets.Simulation.Units;

namespace Assets.UI.Cities.Production {

    public class CityProjectRecord : MonoBehaviour {

        #region instance fields and properties

        public IUnitTemplate UnitTemplate { get; set; }

        public IBuildingTemplate BuildingTemplate { get; set; }

        public Button SelectionButton {
            get { return _selectionButton; }
        }
        [SerializeField] private Button _selectionButton;

        [SerializeField] private Text NameField;
        [SerializeField] private TextMeshProUGUI CostField;



        private IYieldFormatter YieldFormatter;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(IYieldFormatter yieldFormatter) {
            YieldFormatter = yieldFormatter;
        }

        public void Refresh() {
            if(UnitTemplate != null) {
                NameField.text = UnitTemplate.Name;
                CostField.text = YieldFormatter.GetTMProFormattedSingleResourceString(
                    ResourceType.Production, UnitTemplate.ProductionCost
                );

            }else if(BuildingTemplate != null) {
                NameField.text = BuildingTemplate.name;
                CostField.text = BuildingTemplate.Cost.ToString();
            }
        }

        #endregion

    }

}
