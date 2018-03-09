using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.Core;
using Assets.Simulation;

using UnityCustomUtilities.Extensions;

namespace Assets.UI {

    public class YieldFormatter : IYieldFormatter {

        #region instance fields and properties

        private ICoreConfig CoreConfig;

        #endregion

        #region constructors

        public YieldFormatter(ICoreConfig coreConfig) {
            CoreConfig = coreConfig;
        }

        #endregion

        #region instance methods

        public string GetTMProFormattedYieldString(ResourceSummary summary, bool includeEmptyValues = false) {
            if(includeEmptyValues) {
                return GetTMProFormattedYieldString(
                    summary, EnumUtil.GetValues<ResourceType>()
                );
            }else {
                return GetTMProFormattedYieldString(
                    summary, EnumUtil.GetValues<ResourceType>().Where(type => summary[type] != 0f)
                );
            }
        }

        private string GetTMProFormattedYieldString(ResourceSummary summary, IEnumerable<ResourceType> typesToDisplay) {
            string summaryString = "";

            foreach(var resourceType in typesToDisplay) {
                summaryString += String.Format(
                    "<color=#{0}>{1:+0;-#} <sprite index={2}>",
                    ColorUtility.ToHtmlStringRGB(CoreConfig.GetColorForResourceType(resourceType)),
                    summary[resourceType],
                    (int)resourceType
                );

                if(resourceType != typesToDisplay.Last()) {
                    summaryString += "<color=\"black\">, ";
                }
            }

            return summaryString;
        }

        #endregion

    }

}
