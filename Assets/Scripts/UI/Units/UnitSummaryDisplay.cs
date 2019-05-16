using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;
using UniRx;

using Assets.Simulation.Units;
using Assets.Simulation.Units.Promotions;

namespace Assets.UI.Units {

    public class UnitSummaryDisplay : UnitDisplayBase {

        #region instance fields and properties

        [SerializeField] private Text NameField;
        [SerializeField] private Text TypeField;
        [SerializeField] private Text CurrentMovementField;
        [SerializeField] private Text MaxMovementField;
        [SerializeField] private Text ExperienceField;
        [SerializeField] private Text ExperienceForNextLevelField;

        [SerializeField] private Slider HealthSlider;

        private IDisposable ExperienceChangedSubscription;
        private IDisposable LevelChangedSubscription;




        private IUnitConfig          Config;
        private IUnitExperienceLogic UnitExperienceLogic;
        private UnitSignals          Signals;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IUnitConfig config, IUnitExperienceLogic unitExperienceLogic,
            UnitSignals signals
        ){
            Config              = config;
            UnitExperienceLogic = unitExperienceLogic;
            Signals             = signals;
        }

        #region from UnitDisplayBase

        protected override void DoOnEnable() {
            ExperienceChangedSubscription = Signals.ExperienceChanged.Subscribe(OnExperienceChanged);
            LevelChangedSubscription      = Signals.LevelChanged     .Subscribe(OnLevelChanged);
        }

        protected override void DoOnDisable() {
            ExperienceChangedSubscription.Dispose();
            LevelChangedSubscription     .Dispose();
        }

        public override void Refresh() {
            if(ObjectToDisplay == null) {
                return;
            }

            int experienceForNextLevel = UnitExperienceLogic.GetExperienceForNextLevelOnUnit(ObjectToDisplay);

            NameField                  .text = ObjectToDisplay.Name;
            TypeField                  .text = ObjectToDisplay.Type           .ToString();
            CurrentMovementField       .text = ObjectToDisplay.CurrentMovement.ToString();
            MaxMovementField           .text = ObjectToDisplay.MaxMovement    .ToString();
            ExperienceField            .text = ObjectToDisplay.Experience     .ToString();
            ExperienceForNextLevelField.text = experienceForNextLevel         .ToString();

            HealthSlider.minValue = 0;
            HealthSlider.maxValue = Config.MaxHealth;
            HealthSlider.value    = ObjectToDisplay.CurrentHitpoints;
        }

        #endregion

        private void OnExperienceChanged(IUnit unit) {
            if(unit == ObjectToDisplay) {
                Refresh();
            }
        }

        private void OnLevelChanged(IUnit unit) {
            if(unit == ObjectToDisplay) {
                Refresh();
            }
        }

        #endregion

    }

}
