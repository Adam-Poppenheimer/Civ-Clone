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



        private IYieldConfig YieldConfig;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(IYieldConfig yieldConfig) {
            YieldConfig = yieldConfig;
        }

        #region from IResourceSummaryDisplay

        public void DisplaySummary(ResourceSummary summary) {
            string summaryString = "";

            var resourceTypes = EnumUtil.GetValues<ResourceType>();

            foreach(var resourceType in resourceTypes) {
                summaryString += String.Format(
                    "<sprite index={0}> <color=#{1}>{2}",
                    (int)resourceType,
                    ColorUtility.ToHtmlStringRGB(YieldConfig.GetColorForResourceType(resourceType)),
                    summary[resourceType]
                );

                if(resourceType != resourceTypes.Last()) {
                    summaryString += "<color=\"black\">, ";
                }
            }
            
            SummaryField.text = summaryString;
        }

        #endregion

        #endregion
        
    }

}
