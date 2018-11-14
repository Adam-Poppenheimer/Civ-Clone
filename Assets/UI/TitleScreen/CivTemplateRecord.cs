using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Assets.Simulation.Civilizations;

namespace Assets.UI.TitleScreen {

    public class CivTemplateRecord : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private Dropdown TemplateDropdown;

        public ICivilizationTemplate SelectedTemplate { get; private set; }

        private ReadOnlyCollection<ICivilizationTemplate> CivTemplates;

        #endregion

        #region instance methods

        public void Refresh(ReadOnlyCollection<ICivilizationTemplate> availableTemplates, int startingOptionIndex) {
            SetDropdownOptions(availableTemplates, startingOptionIndex);

            UpdateSelectedTemplate(startingOptionIndex);
        }

        public void UpdateSelectedTemplate(int optionIndex) {
            var templateName = TemplateDropdown.options[optionIndex].text;

            SelectedTemplate = CivTemplates.Where(template => template.Name.Equals(templateName)).FirstOrDefault();
        }

        private void SetDropdownOptions(ReadOnlyCollection<ICivilizationTemplate> availableTemplates, int startingOptionIndex) {
            CivTemplates = availableTemplates;

            TemplateDropdown.ClearOptions();

            List<Dropdown.OptionData> options = availableTemplates.Select(template => new Dropdown.OptionData(template.Name)).ToList();

            TemplateDropdown.options = options;
            TemplateDropdown.value = startingOptionIndex;
        }

        #endregion

    }

}
