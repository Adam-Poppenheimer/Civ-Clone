using System;
using System.Collections.Generic;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.MapGeneration {

    public interface IMapGenerationVariables {

        #region properties

        int ChunkCountX { get; }
        int ChunkCountZ { get; }

        int ContinentalLandPercentage { get; }

        List<ICivilizationTemplate> Civilizations { get; }

        #endregion

    }

}