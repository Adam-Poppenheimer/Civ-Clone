using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Units;
using Assets.Simulation.HexMap;
using Assets.Simulation.AI;

namespace Assets.Simulation.Barbarians {

    //This class exists primarily to make unit testing easier
    public class BarbarianBrainTools : IBarbarianBrainTools {

        #region instance fields and properties

        private IHexGrid           Grid;
        private IUnitPositionCanon UnitPositionCanon;
        private IBarbarianConfig   BarbarianConfig;

        #endregion

        #region constructors

        [Inject]
        public BarbarianBrainTools(
            IHexGrid grid, IUnitPositionCanon unitPositionCanon, IBarbarianConfig barbarianConfig
        ) {
            Grid              = grid;
            UnitPositionCanon = unitPositionCanon;
            BarbarianConfig   = barbarianConfig;
        }

        #endregion

        #region instance methods

        public Func<IHexCell, float> GetPillageUtilityFunction(IUnit unit, InfluenceMaps maps) {
            var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

            return delegate(IHexCell cell) {
                float divisor = Grid.GetDistance(unitLocation, cell) + 1;

                return Mathf.Clamp01(maps.PillagingValue[cell.Index] * BarbarianConfig.PillageUtilityCoefficient / divisor);
            };
        }

        public Func<IHexCell, int> GetWanderWeightFunction(IUnit unit, InfluenceMaps maps) {
            var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

            return delegate(IHexCell cell) {
                if(cell == unitLocation || !UnitPositionCanon.CanPlaceUnitAtLocation(unit, cell, false)) {
                    return 0;
                }else {
                    float fromDistance = Grid.GetDistance(cell, unitLocation) * BarbarianConfig.WanderSelectionWeight_Distance;
                    float fromAllies   = -maps.AllyPresence[cell.Index]       * BarbarianConfig.WanderSelectionWeight_Allies;

                    return Math.Max(0, Mathf.RoundToInt(fromDistance + fromAllies));
                }
            };
        }

        public Func<IHexCell, int> GetPillageWeightFunction(IUnit unit, InfluenceMaps maps) {
            var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

            return delegate(IHexCell cell) {
                return Mathf.RoundToInt(maps.PillagingValue[cell.Index] / (1 + Grid.GetDistance(unitLocation, cell)));
            };
        }

        #endregion

    }

}
