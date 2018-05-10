using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;
using UniRx;

using Assets.Simulation.Units;

namespace Assets.UI.MapEditor {

    public class UnitEditingPanel : MonoBehaviour {

        #region instance fields and properties

        public IUnit UnitToEdit { get; set; }

        [SerializeField] private Text NameField;

        [SerializeField] private Text   HitpointsField;
        [SerializeField] private Slider HitpointsSlider;

        [SerializeField] private Text   CurrentMovementField;
        [SerializeField] private Slider CurrentMovementSlider;

        [SerializeField] private Text   ExperienceField;
        [SerializeField] private Slider ExperienceSlider;

        [SerializeField] private Toggle SetUpToBombardToggle;

        private IDisposable SetUpForBombardmentSubscription;
        private IDisposable BecameIdleSubscription;




        private UnitSignals UnitSignals;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(UnitSignals unitSignals) {
            UnitSignals = unitSignals;
        }

        #region Unity messages

        private void OnEnable() {
            Refresh();

            SetUpForBombardmentSubscription = UnitSignals.SetUpForBombardmentSignal.Subscribe(OnUnitSetUpForBombardment);
            BecameIdleSubscription          = UnitSignals.BecameIdleSignal         .Subscribe(OnUnitBecameIdle);
        }

        private void OnDisable() {
            SetUpForBombardmentSubscription.Dispose();
            BecameIdleSubscription         .Dispose();
        }

        #endregion

        public void Refresh() {
            if(UnitToEdit == null) {
                return;
            }

            NameField .text = UnitToEdit.Name;

            HitpointsField.text = UnitToEdit.Hitpoints.ToString();

            HitpointsSlider.minValue = 0;
            HitpointsSlider.maxValue = UnitToEdit.MaxHitpoints;
            HitpointsSlider.value    = UnitToEdit.Hitpoints;

            CurrentMovementField.text = UnitToEdit.CurrentMovement.ToString();

            CurrentMovementSlider.minValue = 0;
            CurrentMovementSlider.maxValue = UnitToEdit.MaxMovement;
            CurrentMovementSlider.value    = UnitToEdit.CurrentMovement;

            ExperienceField.text = UnitToEdit.Experience.ToString();

            ExperienceSlider.minValue = 0;
            ExperienceSlider.value    = UnitToEdit.Experience;

            if(UnitToEdit.Template.MustSetUpToBombard) {
                SetUpToBombardToggle.gameObject.SetActive(true);
                SetUpToBombardToggle.isOn = UnitToEdit.IsSetUpToBombard;
            }else {
                SetUpToBombardToggle.gameObject.SetActive(false);
            }
        }

        public void UpdateHitpoints(float newValue) {
            if(UnitToEdit == null) {
                return;
            }

            UnitToEdit.Hitpoints = Mathf.RoundToInt(newValue);

            Refresh();
        }

        public void UpdateCurrentMovement(float newValue) {
            if(UnitToEdit == null) {
                return;
            }

            UnitToEdit.CurrentMovement = newValue;

            Refresh();
        }

        public void UpdateExperience(float newValue) {
            if(UnitToEdit == null) {
                return;
            }

            UnitToEdit.Experience = Mathf.RoundToInt(newValue);

            Refresh();
        }

        public void UpdateBombardStatus(bool isSetUp) {
            if(UnitToEdit == null) {
                return;
            }

            if(isSetUp) {
                UnitToEdit.SetUpToBombard();
            }else {
                UnitToEdit.BeginIdling();
            }
        }

        private void OnUnitSetUpForBombardment(IUnit unit) {
            if(unit == UnitToEdit) {
                Refresh();
            }
        }

        private void OnUnitBecameIdle(IUnit unit) {
            if(unit == UnitToEdit) {
                Refresh();
            }
        }

        #endregion

    }

}
