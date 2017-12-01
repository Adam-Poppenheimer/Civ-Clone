using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Distribution;

namespace Assets.UI.Cities.Distribution {

    public class CityUnemploymentDisplay : CityDisplayBase {

        #region instance fields and properties

        [SerializeField] private Text UnemployedPeopleField;

        private IWorkerDistributionLogic DistributionLogic;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IWorkerDistributionLogic distributionLogic,
            [InjectOptional(Id = "Unemployed People Field")] Text unemployedPeopleField
        ) {
            DistributionLogic = distributionLogic;

            UnemployedPeopleField = unemployedPeopleField != null ? unemployedPeopleField : UnemployedPeopleField;
        }

        #region from CityDisplayBase

        public override void Refresh() {
            if(CityToDisplay == null) {
                return;
            }
            UnemployedPeopleField.text = DistributionLogic.GetUnemployedPeopleInCity(CityToDisplay).ToString();
        }

        #endregion

        #endregion

    }

}
