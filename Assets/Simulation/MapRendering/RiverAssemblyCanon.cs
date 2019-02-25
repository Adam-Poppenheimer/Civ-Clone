using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;
using System.Collections.ObjectModel;

namespace Assets.Simulation.MapRendering {

    public class RiverAssemblyCanon : IRiverAssemblyCanon {

        #region instance fields and properties

        #region from IRiverBuilder

        public ReadOnlyCollection<ReadOnlyCollection<RiverSection>> Rivers {
            get { return rivers.AsReadOnly(); }
        }
        private List<ReadOnlyCollection<RiverSection>> rivers = new List<ReadOnlyCollection<RiverSection>>();

        public IEnumerable<RiverSection> UnassignedSections {
            get { return unassignedSections; }
        }

        #endregion

        private HashSet<RiverSection> unassignedSections = new HashSet<RiverSection>();




        private IRiverSectionCanon SectionCanon;
        private IRiverBuilder      RiverBuilder;

        #endregion

        #region constructors

        [Inject]
        public RiverAssemblyCanon(IRiverSectionCanon sectionCanon, IRiverBuilder riverBuilder) {
            SectionCanon = sectionCanon;
            RiverBuilder = riverBuilder;
        }

        #endregion

        #region instance methods

        #region from IRiverAssemblyCanon

        //There are some river configurations that cause problems here,
        //particularly rivers without endpoints (circular river networks)
        //or pretty much any time a cell is completely surrounded by rivers.
        //It's currently thought that this configuration won't arise often
        //enough to warrant a solution. Circular rivers and many "island"
        //cells are probably a map generation bug, anyways.
        public void RefreshRivers() {
            rivers.Clear();

            SectionCanon.RefreshRiverSections();

            unassignedSections = new HashSet<RiverSection>(SectionCanon.Sections);

            while(unassignedSections.Count > 0) {
                RiverSection unassignedEndpoint = unassignedSections.FirstOrDefault(
                    section => section.HasPreviousEndpoint || section.HasNextEndpoint
                );

                unassignedSections.Remove(unassignedEndpoint);

                if(unassignedEndpoint == null) {
                    break;
                }

                var newRiver = RiverBuilder.BuildRiverFromSection(unassignedEndpoint, unassignedSections);

                rivers.Add(newRiver.AsReadOnly());
            }
        }

        #endregion

        #endregion

    }
}
