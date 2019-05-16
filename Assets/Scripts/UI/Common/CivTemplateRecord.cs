using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Assets.Simulation.Civilizations;

namespace Assets.UI.Common {

    public class CivTemplateRecord : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private Dropdown ValidTemplatesDropdown;

        public string SelectedDropdownText {
            get { return ValidTemplatesDropdown.options[ValidTemplatesDropdown.value].text; }
        }

        #endregion

        #region instance methods

        public void PopulateValidTemplatesDropdown(
            IEnumerable<ICivilizationTemplate> allTemplates, HashSet<ICivilizationTemplate> chosenTemplates,
            ICivilizationTemplate selectedTemplate
        ) {
            ValidTemplatesDropdown.ClearOptions();

            var validTemplates = allTemplates.Where(
                template => template == selectedTemplate || !chosenTemplates.Contains(template)
            );

            List<Dropdown.OptionData> options = validTemplates.Select(
                template => new Dropdown.OptionData(template.Name)
            ).ToList();

            ValidTemplatesDropdown.AddOptions(options);

            var selectedDropdown = ValidTemplatesDropdown.options.Where(
                option => option.text.Equals(selectedTemplate.Name)
            ).FirstOrDefault();

            if(selectedDropdown != null) {
                ValidTemplatesDropdown.value = ValidTemplatesDropdown.options.IndexOf(selectedDropdown);
            }
        }

        #endregion

    }

}
