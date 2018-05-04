﻿using System;

namespace Assets.Simulation.MapManagement {

    public interface IImprovementComposer {

        #region methods

        void ClearRuntime();

        void ComposeImprovements  (SerializableMapData mapData);
        void DecomposeImprovements(SerializableMapData mapData);

        #endregion

    }

}