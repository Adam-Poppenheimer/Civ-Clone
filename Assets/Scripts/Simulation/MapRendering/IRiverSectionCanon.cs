using System.Collections.ObjectModel;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public interface IRiverSectionCanon {

        #region properties

        ReadOnlyCollection<RiverSection> Sections { get; }

        #endregion

        #region methods

        bool AreSectionFlowsCongruous(RiverSection thisSection, RiverSection otherSection);

        RiverSection GetSectionBetweenCells(IHexCell cellOne, IHexCell cellTwo);

        bool IsNeighborOf(RiverSection thisSection, RiverSection otherSection);

        void RefreshRiverSections();

        #endregion

    }

}