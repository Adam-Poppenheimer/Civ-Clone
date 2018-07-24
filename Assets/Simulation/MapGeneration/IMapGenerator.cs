using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.MapGeneration {

    public interface IMapGenerator {

        #region methods

        void GenerateMap(int chunkCountX, int chunkCountZ, IMapGenerationTemplate template);

        #endregion

    }

}
