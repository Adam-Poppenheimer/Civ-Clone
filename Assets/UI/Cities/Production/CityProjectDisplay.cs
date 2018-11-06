using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;
using TMPro;

using UniRx;

using Assets.Simulation;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Production;
using Assets.Simulation.Core;

namespace Assets.UI.Cities.Production {

    public class CityProjectDisplay : CityDisplayBase {

        #region instance fields and properties

        [SerializeField] private Text ProjectNameField;
        [SerializeField] private TextMeshProUGUI ProjectCostField;
        [SerializeField] private Text TurnsLeftField;

        [SerializeField] private Slider ProductionProgressSlider;
        [SerializeField] private Image ProgressSliderFill;

        private List<IDisposable> SignalSubscriptions = new List<IDisposable>();




        private IProductionLogic ProductionLogic;
        private IYieldFormatter  YieldFormatter;
        private CitySignals      CitySignals;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IProductionLogic productionLogic, IYieldFormatter yieldFormatter,
            CitySignals citySignals, ICoreConfig coreConfig
        ){
            ProductionLogic = productionLogic;
            YieldFormatter  = yieldFormatter;
            CitySignals     = citySignals;          

            ProgressSliderFill.color = coreConfig.GetColorForYieldType(YieldType.Production);
        }

        #region from CityDisplayBase

        protected override void DoOnEnable() {
            SignalSubscriptions.Add(CitySignals.ProjectChangedSignal       .Subscribe(OnProjectChanged));
            SignalSubscriptions.Add(CitySignals.DistributionPerformedSignal.Subscribe(OnDistributionPerformed));
        }

        protected override void DoOnDisable() {
            foreach(var subscription in SignalSubscriptions) {
                subscription.Dispose();
            }

            SignalSubscriptions.Clear();
        }

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

        private void DisplayProjectOfCity(ICity city) {
            var project = city.ActiveProject;

            int productionPerTurn = ProductionLogic.GetProductionProgressPerTurnOnProject(city, project);
            int productionLeft = project.ProductionToComplete - project.Progress;

            TurnsLeftField.text = Mathf.CeilToInt((float)productionLeft / productionPerTurn).ToString();

            ProjectNameField.text = project.Name;
            ProjectCostField.text = YieldFormatter.GetTMProFormattedSingleResourceString(
                YieldType.Production, project.ProductionToComplete
            );            

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

        private void OnProjectChanged(Tuple<ICity, IProductionProject> cityProjectTuple) {
            if(cityProjectTuple.Item1.Equals(ObjectToDisplay)) {
                Refresh();
            }
        }

        private void OnDistributionPerformed(ICity city) {
            if(city == ObjectToDisplay) {
                Refresh();
            }
        }

        #endregion

    }

}
