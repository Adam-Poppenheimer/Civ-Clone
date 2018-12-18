using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;
using TMPro;

using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Units.Promotions;
using Assets.Simulation.SocialPolicies;

namespace Assets.UI {

    public class DescriptionTooltip : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private TextMeshProUGUI DescriptionField;
        [SerializeField] private RectTransform   RectTransform;
        [SerializeField] private Canvas          ParentCanvas;
        [SerializeField] private CanvasScaler    CanvasScaler;

        #endregion

        #region instance methods

        #region Unity messages

        private void Start() {
            UpdatePosition();
        }

        private void Update() {
            UpdatePosition();
        }

        #endregion

        public void SetDescriptionFrom(string description) {
            DescriptionField.text = description;
        }

        public void SetDescriptionFrom(IBuildingTemplate template) {
            DescriptionField.text = template.Description;
        }

        public void SetDescriptionFrom(IPromotion promotion) {
            DescriptionField.text = string.Format("{0}\n\n{1}", promotion.name, promotion.Description);
        }

        public void SetDescriptionFrom(ISocialPolicyDefinition policy) {
            DescriptionField.text = string.Format("{0}\n\n{1}", policy.name, policy.Description);
        }

        //Both the canvas and the description tooltip should have their pivot in their center.
        private void UpdatePosition() {
            Vector3 desiredScreenPosition = Input.mousePosition;

            var rectOfCanvas = ParentCanvas.pixelRect;

            float screenWidthRatio  = Screen.width  / CanvasScaler.referenceResolution.x;
            float screenHeightRatio = Screen.height / CanvasScaler.referenceResolution.y;

            float tooltipScreenWidth  = RectTransform.rect.width * screenWidthRatio;
            float tooltipScreenHeight = RectTransform.rect.height * screenHeightRatio;

            float xMin = tooltipScreenWidth / 2f;
            float xMax = rectOfCanvas.width - tooltipScreenWidth / 2f;

            float yMin = tooltipScreenHeight / 2f;
            float yMax = rectOfCanvas.height - tooltipScreenHeight / 2f;

            RectTransform.position = new Vector2(
                Mathf.Clamp(desiredScreenPosition.x, xMin, xMax),
                Mathf.Clamp(desiredScreenPosition.y, yMin, yMax)
            );
        }

        #endregion

    }

}
