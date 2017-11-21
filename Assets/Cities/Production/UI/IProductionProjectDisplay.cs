using System;

namespace Assets.Cities.Production.UI {

    public interface IProductionProjectDisplay {

        #region methods

        void ClearDisplay();
        void DisplayProject(IProductionProject project, int turnsLeft);

        #endregion

    }

}