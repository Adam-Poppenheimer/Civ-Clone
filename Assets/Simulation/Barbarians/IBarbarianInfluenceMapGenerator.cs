using System;

namespace Assets.Simulation.Barbarians {

    public interface IBarbarianInfluenceMapGenerator {

        #region methods

        void ClearMaps();

        BarbarianInfluenceMaps GenerateMaps();

        #endregion

    }

}