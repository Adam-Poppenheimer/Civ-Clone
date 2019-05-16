using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using TMPro;

using Assets.Simulation.Civilizations;

namespace Assets.UI.Civilizations {

    public class CivGoldenAgeDisplay : CivilizationDisplayBase {

        #region instance fields and properties

        [SerializeField] private TextMeshProUGUI TextField;

        [SerializeField] private string TwoArgNonGoldenAgeFormat;
        [SerializeField] private string OneArgGoldenAgeFormat;




        private IGoldenAgeCanon GoldenAgeCanon;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IGoldenAgeCanon goldenAgeCanon
        ) {
            GoldenAgeCanon = goldenAgeCanon;
        }

        #region CivilizationDisplayBase

        public override void Refresh() {
            if(ObjectToDisplay == null) {
                return;
            }

            if(GoldenAgeCanon.IsCivInGoldenAge(ObjectToDisplay)) {
                TextField.SetText(string.Format(
                    OneArgGoldenAgeFormat,
                    GoldenAgeCanon.GetTurnsLeftOnGoldenAgeForCiv(ObjectToDisplay)
                ));
            }else {
                TextField.SetText(string.Format(
                    TwoArgNonGoldenAgeFormat,
                    GoldenAgeCanon.GetGoldenAgeProgressForCiv(ObjectToDisplay),
                    GoldenAgeCanon.GetNextGoldenAgeCostForCiv(ObjectToDisplay)
                ));
            }
        }

        #endregion

        #endregion

    }

}
