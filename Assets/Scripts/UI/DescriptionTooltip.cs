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
using Assets.Simulation.Units;

namespace Assets.UI {

    public class DescriptionTooltip : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private TextMeshProUGUI DescriptionField = null;
        [SerializeField] private RectTransform   RectTransform    = null;
        [SerializeField] private Canvas          ParentCanvas     = null;
        [SerializeField] private CanvasScaler    CanvasScaler     = null;

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

        public void SetDescriptionFrom(IUnitTemplate template) {
            DescriptionField.text = template.Description;
        }

        public void SetDescriptionFrom(IPromotion promotion) {
            DescriptionField.text = string.Format("{0}\n\n{1}", promotion.name, promotion.Description);
        }

        public void SetDescriptionFrom(ISocialPolicyDefinition policy) {
            DescriptionField.text = string.Format("{0}\n\n{1}", policy.name, policy.Description);
        }

        //This implementation still has a few problems with screen edge border detection.
        //It seems like the farther the window shape is from teh reference resolution the
        //worse things get.
        private void UpdatePosition() {
            Vector3 desiredScreenPosition = Input.mousePosition;

            var rectOfCanvas = ParentCanvas.pixelRect;

            float screenWidthRatio  = Screen.width  / CanvasScaler.referenceResolution.x;
            float screenHeightRatio = Screen.height / CanvasScaler.referenceResolution.y;

            float tooltipScreenWidth  = RectTransform.rect.width * screenWidthRatio;
            float tooltipScreenHeight = RectTransform.rect.height * screenHeightRatio;

            float xMin = tooltipScreenWidth * RectTransform.pivot.x;
            float xMax = rectOfCanvas.width - tooltipScreenWidth * (1 - RectTransform.pivot.x);

            float yMin = tooltipScreenHeight * RectTransform.pivot.y;
            float yMax = rectOfCanvas.height - tooltipScreenHeight * (1 - RectTransform.pivot.y);

            RectTransform.position = new Vector2(
                Mathf.Clamp(desiredScreenPosition.x, xMin, xMax),
                Mathf.Clamp(desiredScreenPosition.y, yMin, yMax)
            );
        }

        #endregion

    }

}
