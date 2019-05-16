using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Visibility {

    public interface IExplorationCanon {

        #region properties

        CellExplorationMode ExplorationMode { get; set; }

        #endregion

        #region methods

        bool IsCellExplored(IHexCell cell);

        bool IsCellExploredByCiv(IHexCell cell, ICivilization civ);

        void SetCellAsExploredByCiv  (IHexCell cell, ICivilization civ);
        void SetCellAsUnexploredByCiv(IHexCell cell, ICivilization civ);

        void Clear();

        #endregion

    }

}
