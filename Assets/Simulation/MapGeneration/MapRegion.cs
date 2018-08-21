using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public class MapRegion {

        #region instance fields and properties

        public ReadOnlyCollection<IHexCell> LandCells {
            get { return _landCells.AsReadOnly(); }
        }
        private List<IHexCell> _landCells;

        public ReadOnlyCollection<IHexCell> WaterCells {
            get { return _waterCells.AsReadOnly(); }
        }
        private List<IHexCell> _waterCells;

        public ReadOnlyCollection<IHexCell> Cells {
            get {
                if(_cells == null) {
                    _cells = LandCells.Concat(WaterCells).ToList();
                }

                return _cells.AsReadOnly();
            }
        }
        private List<IHexCell> _cells;

        #endregion

        #region constructors

        public MapRegion(List<IHexCell> landCells, List<IHexCell> waterCells) {
            _landCells  = landCells;
            _waterCells = waterCells;
        }

        #endregion

    }

}
