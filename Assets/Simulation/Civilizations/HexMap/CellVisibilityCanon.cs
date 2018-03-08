using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.HexMap {

    public class CellVisibilityCanon : ICellVisibilityCanon {

        #region instance fields and properties

        private Dictionary<IHexCell, Dictionary<ICivilization, int>> VisibilityOfCellToCiv =
            new Dictionary<IHexCell, Dictionary<ICivilization, int>>();

        #endregion

        #region constructors

        [Inject]
        public CellVisibilityCanon() {

        }

        #endregion

        #region instance methods

        #region from ICellVisibilityCanon

        public bool IsCellVisibleToCiv(IHexCell cell, ICivilization civ) {
            return GetVisibility(cell, civ) > 0;
        }

        public IEnumerable<IHexCell> GetCellsVisibleToCiv(ICivilization civ) {
            throw new NotImplementedException();
        }

        public void DecreaseVisibilityToCiv(IHexCell cell, ICivilization civ) {
            int visibility = GetVisibility(cell, civ) - 1;
            SetVisibility(cell, civ, visibility);
            if(visibility <= 0) {
                cell.RefreshVisibility();
            }
        }

        public void IncreaseVisibilityToCiv(IHexCell cell, ICivilization civ) {
            int visibility =  GetVisibility(cell, civ) + 1;
            SetVisibility(cell, civ, visibility);
            if(visibility >= 1) {
                cell.RefreshVisibility();
            }
        }

        #endregion

        private int GetVisibility(IHexCell cell, ICivilization civ) {
            Dictionary<ICivilization, int> visibilityDictForCell;

            if(!VisibilityOfCellToCiv.ContainsKey(cell)) {
                visibilityDictForCell = new Dictionary<ICivilization, int>();
                VisibilityOfCellToCiv[cell] = visibilityDictForCell;
            }else {
                visibilityDictForCell = VisibilityOfCellToCiv[cell];
            }

            int retval;
            visibilityDictForCell.TryGetValue(civ, out retval);
            return retval;
        }

        private void SetVisibility(IHexCell cell, ICivilization civ, int value) {
            Dictionary<ICivilization, int> visibilityDictForCell;

            if(!VisibilityOfCellToCiv.ContainsKey(cell)) {
                visibilityDictForCell = new Dictionary<ICivilization, int>();
                VisibilityOfCellToCiv[cell] = visibilityDictForCell;
            }else {
                visibilityDictForCell = VisibilityOfCellToCiv[cell];
            }

            visibilityDictForCell[civ] = value;
        }

        #endregion
        
    }

}
