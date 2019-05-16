using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units.Combat {

    public interface IUnitAttackOrderLogic {

        #region methods

        IUnit GetNextAttackTargetOnCell(IHexCell cell);

        #endregion

    }

}
