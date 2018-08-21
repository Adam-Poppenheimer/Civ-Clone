using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.MapGeneration {

    public class CivHomelandData {

        #region instance fields and properties

        public MapRegion StartingRegion { get; private set; }

        public ReadOnlyCollection<MapRegion> OtherRegions {
            get { return _otherRegions.AsReadOnly(); }
        }
        private List<MapRegion> _otherRegions;

        #endregion

        #region constructors

        public CivHomelandData(MapRegion startingRegion, List<MapRegion> otherRegions) {
            StartingRegion = startingRegion;
            _otherRegions  = otherRegions;
        }

        #endregion

    }

}
