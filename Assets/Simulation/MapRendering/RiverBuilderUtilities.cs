using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public class RiverBuilderUtilities : IRiverBuilderUtilities {

        #region instance fields and properties

        private IHexGrid           Grid;
        private IRiverSectionCanon SectionCanon;

        #endregion

        #region constructors

        [Inject]
        public RiverBuilderUtilities(IHexGrid grid, IRiverSectionCanon sectionCanon) {
            Grid         = grid;
            SectionCanon = sectionCanon;
        }

        #endregion

        #region instance methods

        #region from IRiverBuilderUtilities

        public RiverSection GetNextActiveSectionForRiver(
            RiverSection activeSection, RiverSection lastSection, HashSet<RiverSection> unassignedSections
        ) {
            IHexCell center    = activeSection.AdjacentCellOne;
            IHexCell left      = Grid.GetNeighbor(activeSection.AdjacentCellOne, activeSection.DirectionFromOne.Previous());
            IHexCell right     = activeSection.AdjacentCellTwo;
            IHexCell nextRight = Grid.GetNeighbor(activeSection.AdjacentCellOne, activeSection.DirectionFromOne.Next());

            RiverSection centerLeftSection = SectionCanon.GetSectionBetweenCells(center, left);

            //For a section to be a valid next-step, it must be non-null and unassigned.
            //But we also don't want to grab a river adjacent to the previous river,
            //since this could cause weird behavior (go up a river, and then immediately
            //back down it). We thus forbid the acquisition of such rivers.
            //We also need to take flow into account, since it's not valid for a river
            //to suddenly flip its direction
            if(    centerLeftSection != null
                && unassignedSections.Contains(centerLeftSection)
                && !SectionCanon.IsNeighborOf(centerLeftSection, lastSection)
                && SectionCanon.AreSectionFlowsCongruous(activeSection, centerLeftSection)
            ) {
                return centerLeftSection;
            }

            RiverSection leftRightSection = SectionCanon.GetSectionBetweenCells(left, right);

            if(    leftRightSection != null
                && unassignedSections.Contains(leftRightSection)
                && !SectionCanon.IsNeighborOf(leftRightSection, lastSection)
                && SectionCanon.AreSectionFlowsCongruous(activeSection, leftRightSection)
            ) {
                return leftRightSection;
            }                

            RiverSection centerNextRightSection = SectionCanon.GetSectionBetweenCells(center, nextRight);

            if(    centerNextRightSection != null
                && unassignedSections.Contains(centerNextRightSection)
                && !SectionCanon.IsNeighborOf(centerNextRightSection, lastSection)
                && SectionCanon.AreSectionFlowsCongruous(activeSection, centerNextRightSection)
            ) {
                return centerNextRightSection;
            }

            RiverSection rightNextRightSection = SectionCanon.GetSectionBetweenCells(right, nextRight);

            if(    rightNextRightSection != null
                && unassignedSections.Contains(rightNextRightSection)
                && !SectionCanon.IsNeighborOf(rightNextRightSection, lastSection)
                && SectionCanon.AreSectionFlowsCongruous(activeSection, rightNextRightSection)
            ) {
                return rightNextRightSection;
            }

            return null;
        }

        public void SetCurveStatusOfSection(RiverSection section, List<RiverSection> river) {
            IHexCell center    = section.AdjacentCellOne;
            IHexCell left      = Grid.GetNeighbor(section.AdjacentCellOne, section.DirectionFromOne.Previous());
            IHexCell nextRight = Grid.GetNeighbor(section.AdjacentCellOne, section.DirectionFromOne.Next());

            var centerLeftSection      = SectionCanon.GetSectionBetweenCells(center, left);
            var centerNextRightSection = SectionCanon.GetSectionBetweenCells(center, nextRight);

            section.PreviousOnInternalCurve = centerLeftSection      != null && river.Contains(centerLeftSection);
            section.NextOnInternalCurve     = centerNextRightSection != null && river.Contains(centerNextRightSection);
        }

        #endregion

        #endregion

    }

}
