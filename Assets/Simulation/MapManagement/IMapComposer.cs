using System;

namespace Assets.Simulation.MapManagement {

    public interface IMapComposer {

        #region methods

        void ClearRuntime();

        SerializableMapData ComposeRuntimeIntoData();

        void DecomposeDataIntoRuntime(SerializableMapData mapData, Action performAfterDecomposition = null);

        #endregion

    }

}