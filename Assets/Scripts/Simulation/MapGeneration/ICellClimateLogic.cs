using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public interface ICellClimateLogic {

        #region methods

        float GetTemperatureOfCell  (IHexCell cell);
        float GetPrecipitationOfCell(IHexCell cell);

        void Reset(IMapTemplate newTemplate);

        #endregion

    }

}
