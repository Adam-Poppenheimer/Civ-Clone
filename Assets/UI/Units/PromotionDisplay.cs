using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Zenject;

using Assets.Simulation.Units.Promotions;

namespace Assets.UI.Units {

    public class PromotionDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

        #region instance fields and properties

        public IPromotion         PromotionToDisplay { get; set; }
        public bool               AcceptsInput       { get; set; }
        public Action<IPromotion> InputAction        { get; set; }

        [SerializeField] private Image  IconField;
        [SerializeField] private Button SelectionButton;




        private DescriptionTooltip DescriptionTooltip;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(DescriptionTooltip descriptionTooltip) {
            DescriptionTooltip = descriptionTooltip;
        }

        #region Unity messages

        private void OnEnable() {
            IconField.sprite = PromotionToDisplay.Icon;

            if(SelectionButton != null) {
                SelectionButton.interactable = AcceptsInput;

                SelectionButton.onClick.AddListener(() => InputAction(PromotionToDisplay));
            }
        }

        #endregion

        #region EventSystem handlers

        public void OnPointerEnter(PointerEventData eventData) {
            DescriptionTooltip.SetDescriptionFrom(PromotionToDisplay);

            DescriptionTooltip.gameObject.SetActive(true);
            DescriptionTooltip.transform.position = Input.mousePosition;
        }

        public void OnPointerExit(PointerEventData eventData) {
            DescriptionTooltip.gameObject.SetActive(false);
        }

        #endregion

        #endregion

    }

}
