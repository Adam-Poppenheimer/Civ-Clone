using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.MapGeneration {

    public class MapChunk {

        #region instance fields and properties

        public MapSection Seed;

        public List<MapSection> ContiguousWithSeed;

        public List<MapSection> DiscontiguousWithSeed;

        public IEnumerable<MapSection> Sections {
            get {
                if(_sections == null) {
                    _sections = ContiguousWithSeed.Concat(DiscontiguousWithSeed).ToList();
                }
                return _sections;
            }
        }
        private List<MapSection> _sections;

        #endregion

    }

}
