using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using TMPro;

using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Units.Promotions;

namespace Assets.UI {

    public class DescriptionTooltip : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private TextMeshProUGUI DescriptionField;

        #endregion

        #region instance methods

        public void SetDescriptionFrom(string description) {
            DescriptionField.text = description;
        }

        public void SetDescriptionFrom(IBuildingTemplate template) {
            DescriptionField.text = template.Description;
        }

        public void SetDescriptionFrom(IPromotion promotion) {
            DescriptionField.text = string.Format("{0}\n\n{1}", promotion.name, promotion.Description);
        }

        #endregion

    }

}
