using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Players {

    public interface IPlayerBrain {

        #region properties

        string Name { get; }

        #endregion

        #region methods

        void SetUpAnalysis();

        void ExecuteTurn(Action controlRelinquisher);

        void Clear();

        #endregion

    }

}
