using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Units;

namespace Assets.UI.Cities.Production {

    public class CityProjectChooser : CityDisplayBase {

        #region instance fields and properties

        [InjectOptional(Id = "Project Dropdown")]
        private Dropdown ProjectDropdown {
            get { return _projectDropdown; }
            set {
                if(value != null) {
                    _projectDropdown = value;
                    _projectDropdown.onValueChanged.AddListener(OnProjectDropdownChanged);
                }
            }
        }
        [SerializeField] private Dropdown _projectDropdown;

        private IBuildingProductionValidityLogic BuildingValidityLogic;

        private IUnitProductionValidityLogic UnitValidityLogic;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(IBuildingProductionValidityLogic templateValidityLogic,
            IUnitProductionValidityLogic unitValidityLogic
        ){
            BuildingValidityLogic = templateValidityLogic;
            UnitValidityLogic = unitValidityLogic;
        }

        #region Unity message methods

        private void Start() {
            ProjectDropdown.onValueChanged.AddListener(OnProjectDropdownChanged);
        }

        #endregion

        #region from CityDisplayBase

        public override void Refresh() {
            if(ObjectToDisplay == null) {
                return;
            }

            ProjectDropdown.ClearOptions();

            ProjectDropdown.options.Add(new Dropdown.OptionData("None"));

            var validBuildingTemplates = BuildingValidityLogic.GetTemplatesValidForCity(ObjectToDisplay);
            var validUnitTemplates     = UnitValidityLogic    .GetTemplatesValidForCity(ObjectToDisplay);

            var buildingOptions = validBuildingTemplates.Select(template => new Dropdown.OptionData(template.name));
            var unitOptions     = validUnitTemplates    .Select(template => new Dropdown.OptionData(template.Name));

            ProjectDropdown.AddOptions(buildingOptions.Concat(unitOptions).ToList());

            if(ObjectToDisplay.ActiveProject == null) {
                ProjectDropdown.value = 0;
            }else {
                var activeOptionData = ProjectDropdown.options.Where(option => option.text.Equals(ObjectToDisplay.ActiveProject.Name)).FirstOrDefault();
                if(activeOptionData == null) {
                    ProjectDropdown.value = 0;
                }else {
                    ProjectDropdown.value = ProjectDropdown.options.IndexOf(activeOptionData);
                }
            }
        }

        #endregion

        private void OnProjectDropdownChanged(int newValue) {
            if(ObjectToDisplay == null) {
                return;
            }

            var selectedTemplateName = ProjectDropdown.options[newValue].text;

            var validBuildings = BuildingValidityLogic.GetTemplatesValidForCity(ObjectToDisplay);
            var selectedBuilding = validBuildings.Where(template => template.name.Equals(selectedTemplateName)).FirstOrDefault();

            var validUnits = UnitValidityLogic.GetTemplatesValidForCity(ObjectToDisplay);
            var selectedUnit = validUnits.Where(template => template.Name.Equals(selectedTemplateName)).FirstOrDefault();
            
            if(selectedBuilding != null) {
                ObjectToDisplay.SetActiveProductionProject(selectedBuilding);
            }else {
                ObjectToDisplay.SetActiveProductionProject(selectedUnit);
            }
        }

        #endregion

    }

}
