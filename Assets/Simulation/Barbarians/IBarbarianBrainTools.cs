using System;

using Assets.Simulation.HexMap;
using Assets.Simulation.Units;

namespace Assets.Simulation.Barbarians {

    public interface IBarbarianBrainTools {

        #region methods

        Func<IHexCell, int> GetWanderWeightFunction(IUnit unit, BarbarianInfluenceMaps maps);

        #endregion

    }

}