using System;
using System.Collections.Generic;

using Assets.Simulation.HexMap;
using Assets.Simulation.Units;
using Assets.Simulation.AI;

namespace Assets.Simulation.Barbarians {

    public interface IBarbarianSpawningTools {

        #region properties

        Func<IHexCell, bool> EncampmentValidityFilter { get; }

        #endregion

        #region methods

        float GetEncampmentSpawnChance(int encampmentCount, int nonBarbarianCivCount);

        bool IsCellValidForEncampment(IHexCell cell);

        Func<IHexCell, int> BuildEncampmentWeightFunction(InfluenceMaps maps);

        UnitSpawnInfo TryGetValidSpawn(
            IEncampment encampment, Func<IHexCell, IEnumerable<IUnitTemplate>> unitSelector
        );

        #endregion

    }
}