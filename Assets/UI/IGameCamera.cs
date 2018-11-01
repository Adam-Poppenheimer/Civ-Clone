using System;

using Assets.Simulation.HexMap;

namespace Assets.UI {
    public interface IGameCamera {

        #region properties

        bool enabled { get; set; }

        bool SuppressMovemnent { get; set; }
        bool SuppressRotation  { get; set; }
        bool SuppressZoom      { get; set; }

        float Zoom { get; }

        #endregion

        #region methods

        void SnapToCell(IHexCell cell);

        #endregion

    }
}