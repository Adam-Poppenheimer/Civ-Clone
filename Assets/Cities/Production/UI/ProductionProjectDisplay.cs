using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

namespace Assets.Cities.Production.UI {

    public class ProductionProjectDisplay : MonoBehaviour, IProductionProjectDisplay {

        #region instance fields and properties

        [SerializeField] private Text ProjectNameField;
        [SerializeField] private Text ProjectCostField;
        [SerializeField] private Text TurnsLeftField;

        [SerializeField] private Slider ProductionProgressSlider;

        #endregion

        #region instance methods

        #region from IProductionProjectDisplay

        public void DisplayProject(IProductionProject project, int turnsLeft) {
            if(project == null) {
                throw new ArgumentNullException("project");
            }

            ProjectNameField.text = project.Name;
            ProjectCostField.text = project.ProductionToComplete.ToString();

            TurnsLeftField.text = turnsLeft.ToString();

            ProductionProgressSlider.minValue = 0;
            ProductionProgressSlider.maxValue = project.ProductionToComplete;
            ProductionProgressSlider.value = project.Progress;
        }

        public void ClearDisplay() {
            ProjectNameField.text = "--";
            ProjectCostField.text = "--";
            TurnsLeftField  .text = "--";

            ProductionProgressSlider.minValue = 0;
            ProductionProgressSlider.maxValue = 0;
            ProductionProgressSlider.value = 0;
        }

        #endregion

        #endregion

    }

}
