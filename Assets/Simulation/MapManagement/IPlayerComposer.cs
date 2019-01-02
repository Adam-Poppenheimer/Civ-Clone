using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.MapManagement {

    public interface IPlayerComposer {

        #region methods

        void ClearRuntime();

        void ComposePlayers  (SerializableMapData mapData);
        void DecomposePlayers(SerializableMapData mapData);

        #endregion

    }

}
