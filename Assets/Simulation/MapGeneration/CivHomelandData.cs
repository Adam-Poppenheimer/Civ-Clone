using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.MapGeneration {

    public class CivHomelandData {

        #region instance fields and properties

        public MapRegion  StartingRegion { get; private set; }
        public RegionData StartingData   { get; private set; }



        public ReadOnlyCollection<MapRegion> OtherRegions {
            get { return _otherRegions.AsReadOnly(); }
        }
        private List<MapRegion> _otherRegions;

        public ReadOnlyCollection<RegionData> OtherRegionData {
            get { return _otherRegionData.AsReadOnly(); }
        }
        private List<RegionData> _otherRegionData;

        #endregion

        #region constructors

        public CivHomelandData(
            MapRegion startingRegion, RegionData startingData,
            List<MapRegion> otherRegions, List<RegionData> otherRegionData
        ) {
            StartingRegion = startingRegion;
            StartingData   = startingData;

            _otherRegions    = otherRegions;            
            _otherRegionData = otherRegionData;
        }

        #endregion

    }

}
