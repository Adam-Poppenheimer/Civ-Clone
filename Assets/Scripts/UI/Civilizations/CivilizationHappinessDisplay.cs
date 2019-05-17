using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using TMPro;

using Assets.Simulation.Civilizations;

namespace Assets.UI.Civilizations {

    public class CivilizationHappinessDisplay : CivilizationDisplayBase {

        #region instance fields and properties

        [SerializeField] private TextMeshProUGUI HappinessField = null;





        private ICivilizationHappinessLogic CivHappinessLogic;
        private IYieldFormatter             YieldFormatter;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            ICivilizationHappinessLogic civHappinessLogic, IYieldFormatter yieldFormatter
        ){
            CivHappinessLogic = civHappinessLogic;
            YieldFormatter    = yieldFormatter;
        }

        #region CivilizationDisplayBase

        public override void Refresh() {
            if(ObjectToDisplay != null) {
                HappinessField.text = YieldFormatter.GetTMProFormattedHappinessString(
                    CivHappinessLogic.GetNetHappinessOfCiv(ObjectToDisplay)
                );
            }
        }

        #endregion

        #endregion

    }

}
