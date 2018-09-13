using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.HexMap {

    public interface IFarmTriangulator {

        bool ShouldTriangulateFarm(IHexCell cell);

        void TriangulateFarm(IHexCell cell);

    }

}
