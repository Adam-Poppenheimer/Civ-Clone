using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.MapGeneration {

    public class OceanData {

        #region instance fields and properties

        public ReadOnlyCollection<MapRegion> OceanRegions {
            get { return _oceanRegions.AsReadOnly(); }
        }
        private List<MapRegion> _oceanRegions;

        #endregion

        #region constructors

        public OceanData(List<MapRegion> oceanRegions) {
            _oceanRegions     = oceanRegions;

        }

        #endregion

    }

}
