using System;

namespace Assets.Simulation.Players.Barbarians {

    public interface IBarbarianInfluenceMapGenerator {

        #region methods

        void ClearMaps();

        BarbarianInfluenceMaps GenerateMaps();

        #endregion

    }

}