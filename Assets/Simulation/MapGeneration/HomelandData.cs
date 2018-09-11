using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public class HomelandData {

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

        public IEnumerable<IHexCell> Cells {
            get {
                if(_cells == null) {
                    _cells = AllRegions.SelectMany(region => region.Cells).ToList();
                }

                return _cells;
            }
        }
        private List<IHexCell> _cells;

        public IEnumerable<IHexCell> LandCells {
            get {
                if(_landCells == null) {
                    _landCells = AllRegions.SelectMany(region => region.LandCells).ToList();
                }

                return _landCells;
            }
        }
        private List<IHexCell> _landCells;

        public IEnumerable<IHexCell> WaterCells {
            get {
                if(_waterCells == null) {
                    _waterCells = AllRegions.SelectMany(region => region.WaterCells).ToList();
                }

                return _waterCells;
            }
        }
        private List<IHexCell> _waterCells;


        public IEnumerable<LuxuryResourceData> LuxuryResources { get; private set; }

        public IYieldAndResourcesTemplate YieldAndResources { get; private set; }

        #endregion

        #region constructors

        public HomelandData(
            MapRegion startingRegion, RegionData startingData,
            List<MapRegion> otherRegions, List<RegionData> otherRegionData, 
            IEnumerable<LuxuryResourceData> luxuryResources,
            IYieldAndResourcesTemplate yieldData
        ) {
            StartingRegion = startingRegion;
            StartingData   = startingData;

            _otherRegions    = otherRegions;            
            _otherRegionData = otherRegionData;

            LuxuryResources = luxuryResources;
            YieldAndResources       = yieldData;
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
