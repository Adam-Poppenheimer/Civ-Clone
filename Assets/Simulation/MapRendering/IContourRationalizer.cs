using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public interface IContourRationalizer {

        #region methods

        void RationalizeCellContours(IHexCell center, HexDirection direction);

        #endregion

    }

}