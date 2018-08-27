using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public interface IRiverGenerator {

        #region methods

        void CreateRiversForRegion(
            IEnumerable<IHexCell> landCells, IRegionBiomeTemplate template,
            IEnumerable<IHexCell> oceanCells
        );

        #endregion

    }

}
