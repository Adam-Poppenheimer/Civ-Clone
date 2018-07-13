using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.MapGeneration {

    public interface IRiverGenerator {

        #region methods

        void CreateRivers(List<ClimateData> climate, int landCellCount);

        #endregion

    }

}
