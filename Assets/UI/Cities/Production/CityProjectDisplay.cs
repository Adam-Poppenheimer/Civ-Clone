using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using UniRx;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Production;

namespace Assets.UI.Cities.Production {

    public class CityProjectDisplay : CityDisplayBase {

        #region instance fields and properties

        [SerializeField] private Text ProjectNameField;
        [SerializeField] private Text ProjectCostField;
        [SerializeField] private Text TurnsLeftField;

        [SerializeField] private Slider ProductionProgressSlider;

        private IProductionLogic ProductionLogic;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(IProductionLogic productionLogic,
            CityProjectChangedSignal projectChangedSignal) {

            ProductionLogic = productionLogic;

            projectChangedSignal.AsObservable.Subscribe(OnProjectChanged);
        }

        #region from CityDisplayBase

        protected override void DisplayCity(ICity city) {
            if(city.ActiveProject == null) {
                ClearProjectDisplay();
            }else{
                DisplayProjectOfCity(city);
            }
        }

        #endregion

        #region signal responses

        private void OnProjectChanged(Tuple<ICity, IProductionProject> cityProjectTuple) {
            if(cityProjectTuple.Item1.Equals(CityToDisplay)) {

                if(CityToDisplay.ActiveProject != null) {
                    DisplayProjectOfCity(CityToDisplay);
                }else {
                    ClearProjectDisplay();
                }
                               
            }
        }

        #endregion

        private void DisplayProjectOfCity(ICity city) {
            var project = city.ActiveProject;

            int productionPerTurn = ProductionLogic.GetProductionProgressPerTurnOnProject(city, project);
            int productionLeft = project.ProductionToComplete - project.Progress;

            TurnsLeftField.text = Mathf.CeilToInt((float)productionLeft / productionPerTurn).ToString();

            ProjectNameField.text = project.Name;
            ProjectCostField.text = project.ProductionToComplete.ToString();            

            ProductionProgressSlider.minValue = 0;
            ProductionProgressSlider.maxValue = project.ProductionToComplete;
            ProductionProgressSlider.value = project.Progress;
        }

        private void ClearProjectDisplay() {
            TurnsLeftField.text = "--";

            ProjectNameField.text = "--";
            ProjectCostField.text = "--";            

            ProductionProgressSlider.minValue = 0;
            ProductionProgressSlider.maxValue = 0;
            ProductionProgressSlider.value = 0;
        }

        #endregion

    }

}
