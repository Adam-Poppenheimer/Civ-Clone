using System;
using Assets.Simulation.AI;
using Assets.Simulation.HexMap;
using Assets.Simulation.Units;

namespace Assets.Simulation.Barbarians {

    public interface IBarbarianUtilityLogic {

        Func<IHexCell, float> GetAttackUtilityFunction (IUnit unit, InfluenceMaps maps);
        Func<IHexCell, float> GetPillageUtilityFunction(IUnit unit, InfluenceMaps maps);
    }

}