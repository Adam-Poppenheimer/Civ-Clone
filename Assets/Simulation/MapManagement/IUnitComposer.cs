using System;

namespace Assets.Simulation.MapManagement {

    public interface IUnitComposer {

        #region methods

        void ClearRuntime();

        void ComposeUnits  (SerializableMapData mapData);
        void DecomposeUnits(SerializableMapData mapData);

        #endregion

    }

}