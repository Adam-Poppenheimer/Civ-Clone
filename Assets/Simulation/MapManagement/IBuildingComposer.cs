using System;

namespace Assets.Simulation.MapManagement {

    public interface IBuildingComposer {

        #region methods

        void ClearRuntime();

        void ComposeBuildings  (SerializableMapData mapData);
        void DecomposeBuildings(SerializableMapData mapData);

        #endregion

    }

}