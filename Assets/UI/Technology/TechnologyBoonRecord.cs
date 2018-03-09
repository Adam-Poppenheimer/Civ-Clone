using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using TMPro;

namespace Assets.UI.Technology {

    public class TechnologyBoonRecord : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

        #region instance fields and properties

        public Sprite Icon {
            get { return IconImage.sprite; }
            set { IconImage.sprite = value; }
        }

        public string Description { get; set; }

        [SerializeField] private Image IconImage;

        [SerializeField] private GameObject Tooltip;

        #endregion

        #region instance methods

        #region Unity messages

        private void OnEnable() {
            if(IconImage != null) {
                IconImage.sprite = Icon;
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

                var tooltipField = Tooltip.GetComponentInChildren<TextMeshProUGUI>();

                if(tooltipField != null) {
                    tooltipField.text = Description;
                }
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
