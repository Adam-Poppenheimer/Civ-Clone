using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.MapGeneration {

    public interface IHexMapGenerator {

        #region methods

        void GenerateMap(int chunkCountX, int chunkCountZ);

        #endregion

    }

}
