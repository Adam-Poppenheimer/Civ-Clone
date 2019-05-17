using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Zenject;
using TMPro;

namespace Assets.UI.Technology {

    public class TechnologyBoonRecord : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

        #region instance fields and properties

        public Sprite Icon {
            get { return IconImage.sprite; }
            set { IconImage.sprite = value; }
        }

        public string Description { get; set; }

        [SerializeField] private Image IconImage = null;




        private DescriptionTooltip Tooltip;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(DescriptionTooltip tooltip) {
            Tooltip = tooltip;
        }

        #region Unity messages

        private void OnEnable() {
            if(IconImage != null) {
                IconImage.sprite = Icon;
                IconImage.preserveAspect = true;
            }
        }

        private void OnDisable() {
            if(Tooltip != null) {
                Tooltip.gameObject.SetActive(false);
            }
        }

        #endregion

        #region EventSystem handlers

        public void OnPointerEnter(PointerEventData eventData) {
            if(Tooltip != null) {
                Tooltip.gameObject.SetActive(true);

                Tooltip.transform.position = transform.position;
                Tooltip.SetDescriptionFrom(Description);
            }
        }

        public void OnPointerExit(PointerEventData eventData) {
            if(Tooltip != null) {
                Tooltip.gameObject.SetActive(false);
            }
        }

        #endregion

        #endregion

    }

}
