using System;

namespace Assets.Simulation.MapRendering {

    public interface IRiverContourCuller {

        #region methods

        void CullConfluenceContours();

        #endregion

    }

}