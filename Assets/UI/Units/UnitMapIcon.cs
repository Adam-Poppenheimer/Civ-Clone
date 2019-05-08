using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;

namespace Assets.UI.Units {

    public class UnitMapIcon : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {

        #region internal types

        public class Pool : MonoMemoryPool<UnitMapIcon> {

            protected override void Reinitialize(UnitMapIcon item) {
                item.Clear();
            }

        }

        #endregion

        #region instance fields and properties

        public IUnit         UnitToDisplay { get; set; }
        public ICivilization UnitOwner     { get; set; }

        public RectTransform RectTransform {
            get {
                if(_rectTransform == null) {
                    _rectTransform = GetComponent<RectTransform>();
                }

                return _rectTransform;
            }
        }
        private RectTransform _rectTransform;

        [SerializeField] private Image  IconImage;
        [SerializeField] private Image  BackgroundImage;
        [SerializeField] private Slider HealthSlider;

        


        private UnitSignals UnitSignals;

        #endregion

        #region instance methods

        [Inject]
        private void InjectDependencies(UnitSignals unitSignals) {
            UnitSignals = unitSignals;
        }

        #region EventSystem callbacks

        public void OnPointerClick(PointerEventData eventData) {
            if(UnitToDisplay != null) {
                UnitSignals.Clicked.OnNext(UnitToDisplay);
            }
        }

        public void OnBeginDrag(PointerEventData eventData) {
            if(UnitToDisplay != null) {
                UnitSignals.BeginDrag.OnNext(new UniRx.Tuple<IUnit, PointerEventData>(UnitToDisplay, eventData));
            }
        }

        public void OnDrag(PointerEventData eventData) {
            if(UnitToDisplay != null) {
                UnitSignals.Drag.OnNext(new UniRx.Tuple<IUnit, PointerEventData>(UnitToDisplay, eventData));
            }
        }

        public void OnEndDrag(PointerEventData eventData) {
            if(UnitToDisplay != null) {
                UnitSignals.EndDrag.OnNext(new UniRx.Tuple<IUnit, PointerEventData>(UnitToDisplay, eventData));
            }
        }

        #endregion

        public void Refresh() {
            IconImage.sprite = UnitToDisplay.Template.Icon;

            if(UnitOwner != null) {
                BackgroundImage.color = UnitOwner.Template.Color;
            }

            HealthSlider.minValue = 0;
            HealthSlider.maxValue = UnitToDisplay.MaxHitpoints;
            HealthSlider.value    = UnitToDisplay.CurrentHitpoints;
        }

        public void Clear() {
            UnitToDisplay = null;
            IconImage.sprite = null;

            BackgroundImage.color = Color.magenta;

            HealthSlider.minValue = 0;
            HealthSlider.maxValue = 0;
            HealthSlider.value = 0;
        }

        #endregion

    }

}
