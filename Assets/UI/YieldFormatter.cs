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

        public string GetTMProFormattedYieldString(
            ResourceSummary summary, bool includeEmptyValues = false,
            bool plusOnPositiveNumbers = true
        ){
            if(includeEmptyValues) {
                return GetTMProFormattedYieldString(
                    summary, EnumUtil.GetValues<ResourceType>(), plusOnPositiveNumbers
                );
            }else {
                return GetTMProFormattedYieldString(
                    summary, EnumUtil.GetValues<ResourceType>().Where(type => summary[type] != 0f),
                    plusOnPositiveNumbers
                );
            }
        }

        private string GetTMProFormattedYieldString(
            ResourceSummary summary, IEnumerable<ResourceType> typesToDisplay,
            bool plusOnPositiveNumbers
        ){
            string summaryString = "";

            foreach(var resourceType in typesToDisplay) {
                summaryString += String.Format(
                    plusOnPositiveNumbers ? "<color=#{0}>{1:+0;-#} <sprite index={2}>" : "<color=#{0}>{1} <sprite index={2}>",
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

        public string GetTMProFormattedSingleResourceString(ResourceType type, float value) {
            return String.Format(
                "<color=#{0}>{1} <sprite index={2}>",
                ColorUtility.ToHtmlStringRGB(CoreConfig.GetColorForResourceType(type)),
                value,
                (int)type
            );
        }

        #endregion

    }

}
