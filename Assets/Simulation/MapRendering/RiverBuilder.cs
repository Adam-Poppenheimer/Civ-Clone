using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public class RiverBuilder : IRiverBuilder {

        #region instance fields and properties

        private IRiverBuilderUtilities RiverBuilderUtilities;

        #endregion

        #region constructors

        [Inject]
        public RiverBuilder(IRiverBuilderUtilities riverBuilderUtilities) {
            RiverBuilderUtilities = riverBuilderUtilities;
        }

        #endregion

        #region instance methods

        #region from IRiverBuilder

        public List<RiverSection> BuildRiverFromSection(RiverSection startingSection, HashSet<RiverSection> unassignedRiverSections) {
            if(startingSection == null) {
                throw new ArgumentNullException("startingSection");
            }

            var newRiver = new List<RiverSection>();

            if(startingSection.HasPreviousEndpoint && startingSection.HasNextEndpoint) {
                newRiver.Add(startingSection);

                unassignedRiverSections.Remove(startingSection);

            }else {
                RiverSection lastSection   = null;
                RiverSection activeSection = startingSection;

                do {
                    newRiver.Add(activeSection);

                    unassignedRiverSections.Remove(activeSection);

                    RiverSection oldActiveSection = activeSection;

                    activeSection = RiverBuilderUtilities.GetNextActiveSectionForRiver(activeSection, lastSection, unassignedRiverSections);

                    lastSection = oldActiveSection;

                }while(activeSection != null && !activeSection.HasPreviousEndpoint && !activeSection.HasNextEndpoint);

                if(activeSection != null) {
                    newRiver.Add(activeSection);
                    unassignedRiverSections.Remove(activeSection);
                }

                //Our construction guarantees that we grabbed at least one
                //valid endpoint to start with. But we don't know if we grabbed
                //the upriver endpoint or the downriver one first. If we grabbed
                //the downriver one, we need to reverse the whole river.
                if( (startingSection.HasPreviousEndpoint && startingSection.FlowFromOne == RiverFlow.Counterclockwise) ||
                    (startingSection.HasNextEndpoint     && startingSection.FlowFromOne == RiverFlow.Clockwise)
                ) {
                    newRiver.Reverse();
                }
            }

            //Control point calculations vary based on whether the section is
            //on the inside or the outside of a curve. For straight rivers
            //(which contain a single segment) and endpoints this construction
            //causes an S shape to form, which is considered a reasonable result
            foreach(var section in newRiver) {
                RiverBuilderUtilities.SetCurveStatusOfSection(section, newRiver);
            }

            return newRiver;
        }

        #endregion

        #endregion
        
    }

}
