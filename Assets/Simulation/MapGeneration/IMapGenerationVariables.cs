using System;

namespace Assets.Simulation.MapGeneration {

    public interface IMapGenerationVariables {

        #region properties

        int ChunkCountX { get; }
        int ChunkCountZ { get; }

        int CivCount { get; }

        int ContinentalLandPercentage { get; }

        #endregion

    }

}