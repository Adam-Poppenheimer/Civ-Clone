using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.MapGeneration {

    public class MapGenerationVariables : IMapGenerationVariables {

        #region instance fields and properties

        #region from IMapGenerationVariables

        public int ChunkCountX { get; set; }
        public int ChunkCountZ { get; set; }

        public int ContinentalLandPercentage { get; set; }

        public List<ICivilizationTemplate> Civilizations { get; set; }

        #endregion

        #endregion

        #region constructors

        public MapGenerationVariables() {

        }

        #endregion

    }

}
