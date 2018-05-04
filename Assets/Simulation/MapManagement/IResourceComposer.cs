using System;

namespace Assets.Simulation.MapManagement {

    public interface IResourceComposer {

        #region methods

        void ClearRuntime();

        void ComposeResources  (SerializableMapData mapData);
        void DecomposeResources(SerializableMapData mapData);

        #endregion

    }

}