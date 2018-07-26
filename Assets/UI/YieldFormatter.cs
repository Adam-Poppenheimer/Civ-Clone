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
            YieldSummary summary, bool includeEmptyValues = false,
            bool plusOnPositiveNumbers = true
        ){
            if(includeEmptyValues) {
                return GetTMProFormattedYieldString(
                    summary, EnumUtil.GetValues<YieldType>(), plusOnPositiveNumbers
                );
            }else {
                return GetTMProFormattedYieldString(
                    summary, EnumUtil.GetValues<YieldType>().Where(type => summary[type] != 0f),
                    plusOnPositiveNumbers
                );
            }
        }

        private string GetTMProFormattedYieldString(
            YieldSummary summary, IEnumerable<YieldType> typesToDisplay,
            bool plusOnPositiveNumbers
        ){
            string summaryString = "";

            foreach(var resourceType in typesToDisplay) {
                summaryString += String.Format(
                    plusOnPositiveNumbers ? "<color=#{0}>{1:+0;-#} <sprite index={2}>" : "<color=#{0}>{1} <sprite index={2}>",
                    ColorUtility.ToHtmlStringRGB(CoreConfig.GetColorForYieldType(resourceType)),
                    summary[resourceType],
                    (int)resourceType
                );

                if(resourceType != typesToDisplay.Last()) {
                    summaryString += "<color=\"black\">, ";
                }
            }

            return summaryString;
        }

        public string GetTMProFormattedSingleResourceString(YieldType type, float value) {
            return String.Format(
                "<color=#{0}>{1} <sprite index={2}>",
                ColorUtility.ToHtmlStringRGB(CoreConfig.GetColorForYieldType(type)),
                value,
                (int)type
            );
        }

        public string GetTMProFormattedHappinessString(int netHappiness) {
            var color = netHappiness >= 0 ? CoreConfig.PositiveHappinessColor : CoreConfig.NegativeHappinessColor;
            int index = netHappiness >= 0 ? CoreConfig.HappinessIconIndex     : CoreConfig.UnhappinessIconIndex;

            return String.Format(
                "<color=#{0}>{1} <sprite index={2}>",
                ColorUtility.ToHtmlStringRGB(color), netHappiness, index
            );
        }

        #endregion

    }

}
