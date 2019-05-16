using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Cities {

    public interface ICityLineOfSightLogic {

        #region methods

        IEnumerable<IHexCell> GetCellsVisibleToCity(ICity city);

        #endregion

    }

}
