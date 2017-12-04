using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Distribution;

namespace Assets.UI.Cities.Distribution {

    public class DistributionPreferencesDisplay : CityDisplayBase {

        #region instance fields and properties

        [SerializeField] private Dropdown ResourceFocusDropdown;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies([InjectOptional] Dropdown resourceFocusDropdown) {
            if(resourceFocusDropdown != null) {
                ResourceFocusDropdown = resourceFocusDropdown;
            }

            ResourceFocusDropdown.onValueChanged.AddListener(OnResourceFocusDropdownChanged);
        }

        #region from CityDisplayBase

        public override void Refresh() {
            if(ObjectToDisplay == null) {
                return;
            }

            var currentFocusValue = ResourceFocusDropdown.options
                .Where(option => option.text.Equals(ObjectToDisplay.ResourceFocus.ToString()))
                .FirstOrDefault();

            ResourceFocusDropdown.value = ResourceFocusDropdown.options.IndexOf(currentFocusValue);
        }

        #endregion

        private void OnResourceFocusDropdownChanged(int newValue) {
            var newOption = ResourceFocusDropdown.options[newValue];
            var newFocus = (ResourceFocusType)Enum.Parse(typeof(ResourceFocusType), newOption.text, true);
            ObjectToDisplay.ResourceFocus = newFocus;
            ObjectToDisplay.PerformDistribution();
        }

        #endregion

    }

}
