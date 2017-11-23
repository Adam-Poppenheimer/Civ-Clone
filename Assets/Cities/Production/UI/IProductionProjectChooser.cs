using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Cities.Buildings;

namespace Assets.Cities.Production.UI {

    public interface IProductionProjectChooser {

        #region properties

        string ChosenProjectName { get; }

        #endregion

        #region events

        event EventHandler<EventArgs> NewProjectChosen;

        #endregion

        #region methods

        void SetAvailableBuildingTemplates(List<IBuildingTemplate> templates);

        void SetSelectedTemplateFromProject(IProductionProject project);

        void ClearAvailableProjects();

        #endregion

    }

}
