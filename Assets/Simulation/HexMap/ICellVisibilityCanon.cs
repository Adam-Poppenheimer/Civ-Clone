using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.HexMap {

    public interface ICellVisibilityCanon {

        #region methods

        bool IsCellVisibleToCiv(IHexCell cell, ICivilization civ);

        IEnumerable<IHexCell> GetCellsVisibleToCiv(ICivilization civ);

        void IncreaseVisibilityToCiv(IHexCell cell, ICivilization civ);
        void DecreaseVisibilityToCiv(IHexCell cell, ICivilization civ);

        void ClearVisibility();

        #endregion

    }

}
