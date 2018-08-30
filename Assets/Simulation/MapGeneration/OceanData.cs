using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.MapGeneration {

    public class OceanData {

        #region instance fields and properties

        public ReadOnlyCollection<MapRegion> EmptyOceanRegions {
            get { return _emptyOceanRegions.AsReadOnly(); }
        }
        private List<MapRegion> _emptyOceanRegions;

        public ReadOnlyCollection<MapRegion> ArchipelagoRegions {
            get { return _archipelagoRegions.AsReadOnly(); }
        }
        private List<MapRegion> _archipelagoRegions;

        private List<RegionData> ArchipelagoRegionData;

        #endregion

        #region constructors

        public OceanData(
            List<MapRegion> emptyOceanRegions, List<MapRegion> archipelagoRegions,
            List<RegionData> archipelagoRegionData
        ) {
            _emptyOceanRegions  = emptyOceanRegions;
            _archipelagoRegions = archipelagoRegions;

            ArchipelagoRegionData = archipelagoRegionData;
        }

        #endregion

        #region instance methods

        public RegionData GetRegionData(MapRegion archipelagoRegion) {
            int regionIndex = ArchipelagoRegions.IndexOf(archipelagoRegion);

            if(regionIndex < 0) {
                throw new InvalidOperationException("The given region is not an archipelago region in this ocean");
            }

            return ArchipelagoRegionData[regionIndex];
        }

        #endregion

    }

}
