using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

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
