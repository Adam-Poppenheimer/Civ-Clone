using System;

namespace Assets.Simulation.MapManagement {

    public interface IBarbarianComposer {

        #region methods

        void ClearRuntime();

        void ComposeBarbarians  (SerializableMapData mapData);
        void DecomposeBarbarians(SerializableMapData mapData);

        #endregion

    }

}