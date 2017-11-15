using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Cities {

    public interface ICity {

        #region properties

        int Population { get; set; }

        ReadOnlyCollection<IBuilding> Buildings { get; }

        ResourceSummary Stockpile { get; set; }

        ResourceSummary Income { get; }

        IProductionProject CurrentProject { get; }

        #endregion

        #region instance methods

        void AddBuilding(IBuilding building);
        void RemoveBuilding(IBuilding building);

        void SetCurrentProject(IProductionProject project);

        void PerformBeginningOfTurn();
        void PerformEndOfTurn();

        #endregion

    }

}
