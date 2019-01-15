using System;

using Assets.Simulation.HexMap;
using Assets.Simulation.Units;
using Assets.Simulation.AI;

namespace Assets.Simulation.Barbarians {

    public interface IBarbarianBrainTools {

        #region methods

        Func<IHexCell, float> GetPillageUtilityFunction(IUnit unit, InfluenceMaps maps);

        Func<IHexCell, int> GetWanderWeightFunction (IUnit unit, InfluenceMaps maps);
        Func<IHexCell, int> GetPillageWeightFunction(IUnit unit, InfluenceMaps maps);

        #endregion

    }

}