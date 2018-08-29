using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Assets.Simulation.MapResources;

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

        public IEnumerable<MapRegion> AllRegions {
             get {
                if(_allRegions == null) {
                    _allRegions = new List<MapRegion>(OtherRegions);
                    _allRegions.Add(StartingRegion);
                }

                return _allRegions;
            }
        }
        private List<MapRegion> _allRegions;


        public IEnumerable<LuxuryResourceData> LuxuryResources { get; private set; }

        #endregion

        #region constructors

        public CivHomelandData(
            MapRegion startingRegion, RegionData startingData,
            List<MapRegion> otherRegions, List<RegionData> otherRegionData, 
            IEnumerable<LuxuryResourceData> luxuryResources           
        ) {
            StartingRegion = startingRegion;
            StartingData   = startingData;

            _otherRegions    = otherRegions;            
            _otherRegionData = otherRegionData;

            LuxuryResources = luxuryResources;
        }

        #endregion

        #region methods

        public RegionData GetDataOfRegion(MapRegion region) {
            if(region == StartingRegion) {
                return StartingData;
            }else {
                int index = OtherRegions.IndexOf(region);

                if(index >= 0) {
                    return OtherRegionData[index];
                }else {
                    throw new InvalidOperationException("No data exists for the argued region");
                }
            }
        }

        #endregion

    }

}
