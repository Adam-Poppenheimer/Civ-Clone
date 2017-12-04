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
        public void InjectDependencies(
            IProductionLogic productionLogic, CityProjectChangedSignal projectChangedSignal,
            [InjectOptional(Id = "Project Name Field"        )] Text projectNameField, 
            [InjectOptional(Id = "Project Cost Field"        )] Text projectCostField, 
            [InjectOptional(Id = "Turns Left Field"          )] Text turnsLeftField, 
            [InjectOptional(Id = "Production Progress Slider")] Slider productionProgressSlider
        ){

            ProductionLogic = productionLogic;
            projectChangedSignal.AsObservable.Subscribe(OnProjectChanged);

            if(projectNameField != null) { ProjectNameField         = projectNameField;         }
            if(projectNameField != null) { ProjectCostField         = projectCostField;         }
            if(projectNameField != null) { TurnsLeftField           = turnsLeftField;           }
            if(projectNameField != null) { ProductionProgressSlider = productionProgressSlider; }
        }

        #region from CityDisplayBase

        public override void Refresh() {
            if(ObjectToDisplay == null) {
                return;
            }

            if(ObjectToDisplay.ActiveProject == null) {
                ClearProjectDisplay();
            }else{
                DisplayProjectOfCity(ObjectToDisplay);
            }
        }

        #endregion

        #region signal responses

        private void OnProjectChanged(Tuple<ICity, IProductionProject> cityProjectTuple) {
            if(cityProjectTuple.Item1.Equals(ObjectToDisplay)) {

                if(ObjectToDisplay.ActiveProject != null) {
                    DisplayProjectOfCity(ObjectToDisplay);
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
