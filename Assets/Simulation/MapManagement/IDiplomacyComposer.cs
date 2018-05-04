using System;

namespace Assets.Simulation.MapManagement {

    public interface IDiplomacyComposer {

        #region methods

        void ClearRuntime();

        void ComposeDiplomacy  (SerializableMapData mapData);
        void DecomposeDiplomacy(SerializableMapData mapData);

        #endregion

    }

}