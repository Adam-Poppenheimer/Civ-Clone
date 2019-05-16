using System;

using Assets.Simulation.HexMap;
using Assets.Simulation.Units;
using Assets.Simulation.AI;

namespace Assets.Simulation.Barbarians {

    public interface IBarbarianBrainWeightLogic {

        #region methods

        Func<IHexCell, int>   GetWanderWeightFunction (IUnit unit, InfluenceMaps maps);
        Func<IHexCell, int>   GetPillageWeightFunction(IUnit unit, InfluenceMaps maps);
        Func<IHexCell, float> GetFleeWeightFunction   (IUnit unit, InfluenceMaps maps);

        #endregion

    }

}