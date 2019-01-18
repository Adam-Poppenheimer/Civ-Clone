using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Units;
using Assets.Simulation.Units.Combat;
using Assets.Simulation.HexMap;
using Assets.Simulation.AI;

namespace Assets.Simulation.Barbarians {

    //This class exists primarily to make unit testing easier
    public class BarbarianBrainWeightLogic : IBarbarianBrainWeightLogic {

        #region instance fields and properties

        private IHexGrid               Grid;
        private IUnitPositionCanon     UnitPositionCanon;
        private IBarbarianConfig       BarbarianConfig;
        private IUnitStrengthEstimator UnitStrengthEstimator;

        #endregion

        #region constructors

        [Inject]
        public BarbarianBrainWeightLogic(
            IHexGrid grid, IUnitPositionCanon unitPositionCanon, IBarbarianConfig barbarianConfig,
            IUnitStrengthEstimator unitStrengthEstimator
        ) {
            Grid                  = grid;
            UnitPositionCanon     = unitPositionCanon;
            BarbarianConfig       = barbarianConfig;
            UnitStrengthEstimator = unitStrengthEstimator;
        }

        #endregion

        #region instance methods

        public Func<IHexCell, int> GetWanderWeightFunction(IUnit unit, InfluenceMaps maps) {
            var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

            return delegate(IHexCell cell) {
                if(cell == unitLocation || !UnitPositionCanon.CanPlaceUnitAtLocation(unit, cell, false)) {
                    return 0;
                }else {
                    float fromDistance = Grid.GetDistance(cell, unitLocation) * BarbarianConfig.WanderSelectionWeight_Distance;
                    float fromAllies   = -maps.AllyPresence [cell.Index]      * BarbarianConfig.WanderSelectionWeight_Allies;
                    float fromEnemies  = -maps.EnemyPresence[cell.Index]      * BarbarianConfig.WanderSelectionWeight_Enemies;

                    return Math.Max(0, Mathf.RoundToInt(fromDistance + fromAllies + fromEnemies));
                }
            };
        }

        public Func<IHexCell, int> GetPillageWeightFunction(IUnit unit, InfluenceMaps maps) {
            var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

            return delegate(IHexCell cell) {
                return Mathf.RoundToInt(maps.PillagingValue[cell.Index] / (1 + Grid.GetDistance(unitLocation, cell)));
            };
        }

        public Func<IHexCell, float> GetFleeWeightFunction(IUnit unit, InfluenceMaps maps) {
            var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

            return delegate(IHexCell cell) {
                return UnitStrengthEstimator.EstimateUnitDefensiveStrength(unit, cell)
                     + maps.AllyPresence [cell.Index]
                     - maps.EnemyPresence[cell.Index]
                     + Grid.GetDistance(unitLocation, cell);
            };
        }

        #endregion

    }

}
