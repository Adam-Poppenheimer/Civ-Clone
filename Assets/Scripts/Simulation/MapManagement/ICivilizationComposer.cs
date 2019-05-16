using System;

namespace Assets.Simulation.MapManagement {

    public interface ICivilizationComposer {

        #region methods

        void ClearRuntime();

        void ComposeCivilizations  (SerializableMapData mapData);
        void DecomposeCivilizations(SerializableMapData mapData);

        #endregion

    }

}