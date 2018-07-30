using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.MapResources;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public interface IContinentGenerator {

        #region methods

        void GenerateContinent(
            MapRegion continent, IContinentGenerationTemplate template,
            IEnumerable<IHexCell> oceanCells
        );

        #endregion

    }

}
