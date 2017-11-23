using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Cities.Buildings;

namespace Assets.Cities.Production.UI {

    public class ProductionProjectChooser : MonoBehaviour, IProductionProjectChooser {

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

        public void Start() {
            ProjectDropdown.onValueChanged.AddListener(UpdateChosenProject);
        }

        #endregion

        #region from IProductionProjectChooser

        public void SetAvailableBuildingTemplates(List<IBuildingTemplate> templates) {
            ClearAvailableProjects();

            var dropdownOptions = ProjectDropdown.options;

            dropdownOptions.Add(new Dropdown.OptionData("None"));
            ProjectNameOfOptionIndex[0] = "None";

            for(int i = 0; i < templates.Count(); ++i) {
                var template = templates[i];

                dropdownOptions.Add(new Dropdown.OptionData(template.name));

                ProjectNameOfOptionIndex[i + 1] = template.name;
            }

            ProjectDropdown.RefreshShownValue();
        }

        public void SetSelectedTemplateFromProject(IProductionProject project) {
            if(project == null) {
                ProjectDropdown.value = 0;
                return;
            }

            for(int i = 0; i < ProjectDropdown.options.Count; ++i) {
                if(ProjectDropdown.options[i].text.Equals(project.Name)) {
                    ProjectDropdown.value = i;
                    return;
                }
            }
            ProjectDropdown.value = 0;
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
