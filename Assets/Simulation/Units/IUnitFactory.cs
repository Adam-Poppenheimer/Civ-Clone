using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.GameMap;

namespace Assets.Simulation.Units {

    public interface IUnitFactory : IFactory<IMapTile, IUnitTemplate, IUnit> {

        #region properties

        IEnumerable<IUnit> AllUnits { get; }

        #endregion

    }

}
