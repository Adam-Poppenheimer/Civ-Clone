﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;

namespace Assets.UI.Cities.Production {

    public class CityProjectChooser : CityDisplayBase {

        #region instance fields and properties

        [SerializeField] private Dropdown ProjectDropdown;

        private ITemplateValidityLogic TemplateValidityLogic;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(ITemplateValidityLogic templateValidityLogic) {
            TemplateValidityLogic = templateValidityLogic;
        }

        #region Unity message methods

        private void Start() {
            ProjectDropdown.onValueChanged.AddListener(OnProjectDropdownChanged);
        }

        #endregion

        #region from CityDisplayBase

        public override void Refresh() {
            if(CityToDisplay == null) {
                return;
            }

            ProjectDropdown.ClearOptions();

            ProjectDropdown.options.Add(new Dropdown.OptionData("None"));

            var validTemplates = TemplateValidityLogic.GetTemplatesValidForCity(CityToDisplay);
            ProjectDropdown.AddOptions(validTemplates.Select(template => new Dropdown.OptionData(template.name)).ToList());

            if(CityToDisplay.ActiveProject == null) {
                ProjectDropdown.value = 0;
            }else {
                var activeOptionData = ProjectDropdown.options.Where(option => option.text.Equals(CityToDisplay.ActiveProject.Name)).FirstOrDefault();
                if(activeOptionData == null) {
                    ProjectDropdown.value = 0;
                }else {
                    ProjectDropdown.value = ProjectDropdown.options.IndexOf(activeOptionData);
                }
            }
        }

        #endregion

        private void OnProjectDropdownChanged(int newValue) {
            var selectedTemplateName = ProjectDropdown.options[ProjectDropdown.value].text;

            var validTemplates = TemplateValidityLogic.GetTemplatesValidForCity(CityToDisplay);
            var selectedTemplate = validTemplates.Where(template => template.name.Equals(selectedTemplateName)).FirstOrDefault();
            
            CityToDisplay.SetActiveProductionProject(selectedTemplate);
        }

        #endregion

    }

}
