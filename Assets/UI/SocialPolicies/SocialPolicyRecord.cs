using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Zenject;

using Assets.Simulation.SocialPolicies;
using Assets.Simulation.Civilizations;

namespace Assets.UI.SocialPolicies {

    public class SocialPolicyRecord : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

        #region internal types

        public enum StatusType {
            Unlocked, Available, Unavailable
        }

        #endregion

        #region instance fields and properties

        public ISocialPolicyDefinition PolicyToRecord { get; set; }

        public StatusType Status { get; set; }

        public Button SelectionButton {
            get { return _selectionButton; }
        }
        [SerializeField] private Button _selectionButton;

        [SerializeField] private Image IconField;

        [SerializeField] private Color UnlockedColor;
        [SerializeField] private Color AvailableColor;
        [SerializeField] private Color UnavailableColor;




        private DescriptionTooltip DescriptionTooltip;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(DescriptionTooltip descriptionTooltip) {
            DescriptionTooltip = descriptionTooltip;
        }

        #region EventSystem handlers

        public void OnPointerEnter(PointerEventData eventData) {
            DescriptionTooltip.SetDescriptionFrom(PolicyToRecord);

            DescriptionTooltip.gameObject.SetActive(true);
            DescriptionTooltip.transform.position = Input.mousePosition;
        }

        public void OnPointerExit(PointerEventData eventData) {
            DescriptionTooltip.gameObject.SetActive(false);
        }

        #endregion

        public void Refresh() {
            if(PolicyToRecord == null) {
                return;
            }

            IconField.sprite = PolicyToRecord.Icon;

            SelectionButton.interactable = Status == StatusType.Available;

            switch(Status) {
                case StatusType.Unlocked:    IconField.color = UnlockedColor;    break;
                case StatusType.Available:   IconField.color = AvailableColor;   break;
                case StatusType.Unavailable: IconField.color = UnavailableColor; break;
            }
        }

        #endregion

    }

}
