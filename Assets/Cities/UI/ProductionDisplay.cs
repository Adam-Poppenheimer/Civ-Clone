using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Cities.Buildings;
using Assets.Cities.Production;
using Assets.Cities.Production.UI;

namespace Assets.Cities.UI {

    public class ProductionDisplay : IProductionDisplay {

        #region instance fields and properties

        #region from IProductionDisplay

        public ICity CityToDisplay { get; set; }

        #endregion

        private IProductionLogic ProductionLogic;

        private IProductionProjectDisplay ProjectDisplay;

        private IProductionProjectChooser ProjectChooser;

        private ITemplateValidityLogic TemplateValidityLogic;

        #endregion

        #region constructors

        [Inject]
        public ProductionDisplay(IProductionLogic productionLogic, IProductionProjectDisplay projectDisplay,
            IProductionProjectChooser projectChooser, ITemplateValidityLogic templateValidityLogic) {

            ProductionLogic = productionLogic;
            ProjectDisplay = projectDisplay;
            ProjectChooser = projectChooser;
            TemplateValidityLogic = templateValidityLogic;

            projectChooser.NewProjectChosen += ProjectChooser_NewProjectChosen;
        }

        #endregion

        #region instance methods

        #region from IProductionDisplay

        public void Refresh() {
            if(CityToDisplay == null) {
                return;
            }

            if(CityToDisplay.ActiveProject != null) {
                var displayedProject = CityToDisplay.ActiveProject;

                int turnsLeft = Mathf.CeilToInt(
                    (float)displayedProject.ProductionToComplete / ProductionLogic.GetProductionProgressPerTurnOnProject(CityToDisplay, displayedProject)
                );

                ProjectDisplay.DisplayProject(displayedProject, turnsLeft);
            }else {
                ProjectDisplay.ClearDisplay();
            }

            ProjectChooser.ClearAvailableProjects();
            ProjectChooser.SetAvailableBuildingTemplates(TemplateValidityLogic.GetTemplatesValidForCity(CityToDisplay).ToList());
            ProjectChooser.SetSelectedTemplateFromProject(CityToDisplay.ActiveProject);
        }

        #endregion

        private void ProjectChooser_NewProjectChosen(object sender, EventArgs e) {
            if(CityToDisplay == null) {
                return;
            }

            var validTemplates = TemplateValidityLogic.GetTemplatesValidForCity(CityToDisplay);
            var templateOfName = validTemplates.Where(
                template => template.name.Equals(ProjectChooser.ChosenProjectName)
            ).FirstOrDefault();

            CityToDisplay.SetActiveProductionProject(templateOfName);
        }

        #endregion

    }

}
