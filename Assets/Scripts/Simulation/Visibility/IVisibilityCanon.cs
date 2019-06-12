using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;

namespace Assets.Simulation.Visibility {

    public interface IVisibilityCanon {

        #region properties

        CellVisibilityMode     CellVisibilityMode     { get; set; }
        ResourceVisibilityMode ResourceVisibilityMode { get; set; }

        #endregion

        #region methods

        bool IsCellVisible(IHexCell cell);

        bool IsCellVisibleToCiv(IHexCell cell, ICivilization civ);

        int GetVisibilityOfCellToCiv(IHexCell cell, ICivilization civ);

        void IncreaseCellVisibilityToCiv(IHexCell cell, ICivilization civ);
        void DecreaseCellVisibilityToCiv(IHexCell cell, ICivilization civ);

        void ClearCellVisibility();



        bool IsResourceVisible(IResourceDefinition resource);

        #endregion

    }

}
