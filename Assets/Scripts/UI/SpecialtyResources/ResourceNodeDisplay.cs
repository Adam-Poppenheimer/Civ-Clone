using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

using Assets.Simulation.MapResources;

namespace Assets.UI.SpecialtyResources {

    public class ResourceNodeDisplay : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private TextMeshProUGUI SummaryField = null;

        #endregion

        #region instance methods

        public void DisplayNode(IResourceNode node) {
            if(SummaryField != null) {
                string summaryText;

                if(node.Resource.Type == ResourceType.Strategic) {
                    summaryText = String.Format(StringFormats.ResourceNodeSummary_Strategic, node.Resource.name, node.Copies);
                }else {
                    summaryText = String.Format(StringFormats.ResourceNodeSummary_NonStrategic, node.Resource.name);
                }

                SummaryField.text = summaryText;
            }
        }

        #endregion

    }

}
