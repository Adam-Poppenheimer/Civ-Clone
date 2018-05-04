using System;

namespace Assets.Simulation.MapManagement {

    public interface ICityComposer {

        #region methods

        void ClearRuntime();

        void ComposeCities  (SerializableMapData mapData);
        void DecomposeCities(SerializableMapData mapData);

        #endregion

    }

}