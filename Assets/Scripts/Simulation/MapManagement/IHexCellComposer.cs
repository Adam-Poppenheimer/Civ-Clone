using System;

namespace Assets.Simulation.MapManagement {

    public interface IHexCellComposer {

        #region methods

        void ClearRuntime();

        void ComposeCells  (SerializableMapData mapData);
        void DecomposeCells(SerializableMapData mapData);

        #endregion

    }

}