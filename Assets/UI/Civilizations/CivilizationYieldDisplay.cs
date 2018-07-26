using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using TMPro;

using Assets.Simulation;
using Assets.Simulation.Civilizations;

namespace Assets.UI.Civilizations {

    public class CivilizationYieldDisplay : CivilizationDisplayBase {

        #region instance fields and properties

        [SerializeField] private TextMeshProUGUI GoldField;
        [SerializeField] private TextMeshProUGUI CultureField;
        [SerializeField] private TextMeshProUGUI ScienceField;



        private ICivilizationYieldLogic YieldLogic;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(ICivilizationYieldLogic yieldLogic) {
            YieldLogic = yieldLogic;
        }

        #region from CivilizationDisplayBase

        public override void Refresh() {
            if(ObjectToDisplay == null) {
                return;
            }

            var civilizationYield = YieldLogic.GetYieldOfCivilization(ObjectToDisplay);

            GoldField.text = String.Format(
                StringFormats.StockpileAndIncomeDisplayFormat,
                (int)YieldType.Gold,
                ObjectToDisplay.GoldStockpile,
                civilizationYield[YieldType.Gold]
            );

            CultureField.text = String.Format(
                StringFormats.StockpileAndIncomeDisplayFormat,
                (int)YieldType.Culture,
                ObjectToDisplay.CultureStockpile,
                civilizationYield[YieldType.Culture]
            );

            ScienceField.text = String.Format(
                StringFormats.IncomeDisplayFormat,
                (int)YieldType.Science,
                civilizationYield[YieldType.Science]
            );
        }

        #endregion

        #endregion

    }

}
