using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Cities.Buildings;

namespace Assets.Cities.Production.UI {

    public class ProductionProjectChooser : IProductionProjectChooser, IInitializable {

        #region instance fields and properties

        #region from IProductionProjectChooser

        public string ChosenProjectName { get; private set; }

        #endregion

        [SerializeField] private Dropdown ProjectDropdown;

        private Dictionary<int, string> ProjectNameOfOptionIndex =
            new Dictionary<int, string>();

        #endregion

        #region events

        #region from IProductionProjectChooser

        public event EventHandler<EventArgs> NewProjectChosen;

        protected void RaiseNewProjectChosen() {
            if(NewProjectChosen != null) {
                NewProjectChosen(this, EventArgs.Empty);
            }
        }

        #endregion

        #endregion

        #region instance methods

        #region from IInitializable

        public void Initialize() {
            ProjectDropdown.onValueChanged.AddListener(UpdateChosenProject);
        }

        #endregion

        #region from IProductionProjectChooser

        public void SetAvailableBuildingTemplates(List<IBuildingTemplate> templates) {
            ClearAvailableProjects();

            var dropdownOptions = ProjectDropdown.options;

            for(int i = 0; i < templates.Count(); ++i) {
                var template = templates[i];

                dropdownOptions.Add(new Dropdown.OptionData(template.name));

                ProjectNameOfOptionIndex[i] = template.name;
            }

            ProjectDropdown.RefreshShownValue();
        }

        public void ClearAvailableProjects() {
            ProjectDropdown.ClearOptions();
            ProjectNameOfOptionIndex.Clear();
        }

        #endregion

        private void UpdateChosenProject(int optionIndex) {
            ChosenProjectName = ProjectNameOfOptionIndex[optionIndex];
            RaiseNewProjectChosen();
        }

        #endregion

    }

}
