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

    public class YieldSummaryDisplay : MonoBehaviour, IYieldSummaryDisplay {

        #region instance fields and properties

        [SerializeField] private TextMeshProUGUI SummaryField = null;

        [SerializeField] private bool DisplayEmptyResources     = false;
        [SerializeField] private bool PlusSignOnPositiveNumbers = true;
        [SerializeField] private bool IncludeNormalYield        = true;
        [SerializeField] private bool IncludeGreatPersonYield   = true;



        private IYieldFormatter YieldFormatter;
        private ICoreConfig     CoreConfig;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(IYieldFormatter yieldFormatter, ICoreConfig coreConfig) {
            YieldFormatter = yieldFormatter;
            CoreConfig     = coreConfig;
        }

        #region from IResourceSummaryDisplay

        public void DisplaySummary(YieldSummary summary) {
            if(IncludeNormalYield) {
                if(IncludeGreatPersonYield) {
                    SummaryField.text = YieldFormatter.GetTMProFormattedYieldString(
                        summary, EnumUtil.GetValues<YieldType>(), DisplayEmptyResources,
                        PlusSignOnPositiveNumbers
                    );

                }else {
                    SummaryField.text = YieldFormatter.GetTMProFormattedYieldString(
                        summary, CoreConfig.NormalYields, DisplayEmptyResources,
                        PlusSignOnPositiveNumbers
                    );
                }
            }else if(IncludeGreatPersonYield) {
                SummaryField.text = YieldFormatter.GetTMProFormattedYieldString(
                    summary, CoreConfig.GreatPersonYields, DisplayEmptyResources,
                    PlusSignOnPositiveNumbers
                );
            }
            
        }

        #endregion

        #endregion
        
    }

}
