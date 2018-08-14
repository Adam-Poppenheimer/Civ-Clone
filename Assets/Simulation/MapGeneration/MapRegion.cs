using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public class MapRegion {

        #region instance fields and properties

        public readonly MapSection Land;
        public readonly MapSection Water;

        public ReadOnlyCollection<IHexCell> Cells {
            get {
                if(_cells == null) {
                    _cells = Land.Cells.Concat(Water.Cells).ToList();
                }

                return _cells.AsReadOnly();
            }
        }
        private List<IHexCell> _cells;

        #endregion

        #region constructors

        public MapRegion(MapSection land, MapSection water) {
            Land  = land;
            Water = water;
        }

        #endregion

    }

}
