using System;
using System.Collections.Generic;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Technology;

namespace Assets.Simulation.MapGeneration {

    public interface IMapGenerationVariables {

        #region properties

        int CellCountX { get; }
        int CellCountZ { get; }

        int ContinentalLandPercentage { get; }

        List<ICivilizationTemplate> Civilizations { get; }

        IEnumerable<ITechDefinition> StartingTechs { get; }

        #endregion

    }

}