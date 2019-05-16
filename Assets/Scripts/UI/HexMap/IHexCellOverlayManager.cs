using Assets.Simulation.HexMap;

namespace Assets.UI.HexMap {

    public interface IHexCellOverlayManager {

        #region methods

        void ShowOverlayOfCell(IHexCell cell, CellOverlayType type);

        void ClearAllOverlays();

        void ClearOverlay(IHexCell cell);

        #endregion

    }

}