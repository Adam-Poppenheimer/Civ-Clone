using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.HexMap {

    public interface IFreshWaterLogic {

        #region methods

        bool HasAccessToFreshWater(IHexCell cell);

        #endregion

    }

}
