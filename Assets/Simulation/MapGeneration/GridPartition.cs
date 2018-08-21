using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public class GridPartition {

        #region instance fields and properties

        public IEnumerable<MapSection> Sections { get; private set; }

        private Dictionary<MapSection, HashSet<MapSection>> NeighborsOfSection =
            new Dictionary<MapSection, HashSet<MapSection>>();

        private Dictionary<IHexCell, MapSection> SectionOfCell =
            new Dictionary<IHexCell, MapSection>();



        private IHexGrid Grid;

        #endregion

        #region constructors

        public GridPartition(IEnumerable<MapSection> sections, IHexGrid grid) {
            Sections = sections;
            Grid = grid;
        }

        #endregion

        #region instance methods

        public IEnumerable<MapSection> GetNeighbors(MapSection center) {
            HashSet<MapSection> retval;

            if(!NeighborsOfSection.TryGetValue(center, out retval)) {
                retval = new HashSet<MapSection>();

                foreach(var neighboringCell in center.Cells.SelectMany(cell => Grid.GetNeighbors(cell))) {
                    var sectionOfNeighbor = GetSectionOfCell(neighboringCell);

                    if(sectionOfNeighbor != center) {
                        retval.Add(sectionOfNeighbor);
                    }
                }

                NeighborsOfSection[center] = retval;
            }

            return retval;
        }

        public MapSection GetSectionOfCell(IHexCell cell) {
            MapSection retval;

            if(!SectionOfCell.TryGetValue(cell, out retval)) {
                retval = Sections.FirstOrDefault(section => section.Cells.Contains(cell));
                
                SectionOfCell[cell] = retval;
            }

            return retval;
        }

        #endregion

    }

}
