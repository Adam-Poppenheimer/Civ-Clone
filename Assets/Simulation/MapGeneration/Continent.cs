using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.MapGeneration {

    public class Continent {

        #region instance fields and properties

        public MapSection Seed { get; private set; }

        public IEnumerable<MapSection> Sections {
            get { return sections; }
        }
        private HashSet<MapSection> sections = new HashSet<MapSection>();

        #endregion

        #region constructors

        public Continent(MapSection seed) {
            Seed = seed;
            AddSection(seed);
        }

        #endregion

        #region instance methods

        public void AddSection(MapSection section) {
            sections.Add(section);
        }

        #endregion

    }

}
