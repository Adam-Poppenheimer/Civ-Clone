using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;
using TMPro;

using Assets.Simulation;
using Assets.Simulation.Core;

using UnityCustomUtilities.Extensions;

namespace Assets.UI {

    public class ResourceSummaryDisplay : MonoBehaviour, IResourceSummaryDisplay {

        #region instance fields and properties

        [SerializeField] private TextMeshProUGUI SummaryField;

        private IYieldFormatter YieldFormatter;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(IYieldFormatter yieldFormatter) {
            YieldFormatter = yieldFormatter;
        }

        #region from IResourceSummaryDisplay

        public void DisplaySummary(ResourceSummary summary) {
            SummaryField.text = YieldFormatter.GetTMProFormattedYieldString(summary, false);
        }

        #endregion

        #endregion
        
    }

}
