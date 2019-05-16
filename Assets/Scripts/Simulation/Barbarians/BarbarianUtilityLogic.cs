using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.AI;
using Assets.Simulation.HexMap;
using Assets.Simulation.Units;

namespace Assets.Simulation.Barbarians {

    public class BarbarianUtilityLogic : IBarbarianUtilityLogic {

        #region instance fields and properties

        private IUnitPositionCanon UnitPositionCanon;
        private IHexGrid           Grid;
        private IBarbarianConfig   BarbarianConfig;

        #endregion

        #region constructors

        [Inject]
        public BarbarianUtilityLogic(
            IUnitPositionCanon unitPositionCanon, IHexGrid grid, IBarbarianConfig barbarianCOnfig
        ) {
            UnitPositionCanon = unitPositionCanon;
            Grid              = grid;
            BarbarianConfig   = barbarianCOnfig;
        }

        #endregion

        #region instance methods

        #region from IBarbarianUtilityFunctionGenerator

        public Func<IHexCell, float> GetPillageUtilityFunction(IUnit unit, InfluenceMaps maps) {
            var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

            return delegate(IHexCell cell) {
                float divisor = Grid.GetDistance(unitLocation, cell) + 1;

                return Mathf.Clamp01(maps.PillagingValue[cell.Index] * BarbarianConfig.PillageUtilityCoefficient / divisor);
            };
        }

        public Func<IHexCell, float> GetAttackUtilityFunction(IUnit unit, InfluenceMaps maps) {
            return delegate(IHexCell cell) {
                float logisticX = maps.AllyPresence[cell.Index] - maps.EnemyPresence[cell.Index];

                return AIMath.NormalizedLogisticCurve(0f, BarbarianConfig.AttackUtilityLogisticsSlope, logisticX);
            };
        }

        #endregion

        #endregion

    }

}
