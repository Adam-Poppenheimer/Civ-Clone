using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation;
using Assets.Simulation.Civilizations;

namespace Assets.UI.Civilizations {

    public class CivilizationStockpileDisplay : CivilizationDisplayBase {

        #region instance fields and properties

        [SerializeField] private ResourceSummaryDisplay StockpileDisplay;

        #endregion

        #region instance methods

        #region from CivilizationDisplayBase

        public override void Refresh() {
            if(ObjectToDisplay == null) {
                return;
            }

            StockpileDisplay.DisplaySummary(new ResourceSummary(
                gold: ObjectToDisplay.GoldStockpile, culture: ObjectToDisplay.CultureStockpile
            ));
        }

        #endregion

        #endregion

    }

}
