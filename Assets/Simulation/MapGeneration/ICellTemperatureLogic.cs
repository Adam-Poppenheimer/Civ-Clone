using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public interface ICellTemperatureLogic {

        #region methods

        float GetTemperatureOfCell(IHexCell cell, int jitterChannel);

        #endregion

    }

}
