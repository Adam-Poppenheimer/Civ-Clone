using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation;

namespace Assets.UI {

    public class ResourceSummaryDisplay : MonoBehaviour, IResourceSummaryDisplay {

        #region instance fields and properties

        [SerializeField] private Text FoodField;
        [SerializeField] private Text GoldField;
        [SerializeField] private Text ProductionField;
        [SerializeField] private Text CultureField;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            [InjectOptional(Id = "Food Field")]       Text foodField,
            [InjectOptional(Id = "Gold Field")]       Text goldField,
            [InjectOptional(Id = "Production Field")] Text productionField,
            [InjectOptional(Id = "Culture Field")]    Text cultureField
        ) {
            FoodField       = foodField       == null ? FoodField       : foodField;
            GoldField       = goldField       == null ? GoldField       : goldField;
            ProductionField = productionField == null ? ProductionField : productionField;
            CultureField    = cultureField    == null ? CultureField    : cultureField;
        }

        #region from IResourceSummaryDisplay

        public void DisplaySummary(ResourceSummary summary) {
            FoodField      .text = summary[ResourceType.Food      ].ToString();
            GoldField      .text = summary[ResourceType.Gold      ].ToString();
            ProductionField.text = summary[ResourceType.Production].ToString();
            CultureField   .text = summary[ResourceType.Culture   ].ToString();
        }

        #endregion

        #endregion
        
    }

}
