using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Units;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.Players.Barbarians {

    //This class exists primarily to make unit testing easier
    public class BarbarianBrainTools : IBarbarianBrainTools {

        #region instance fields and properties

        private IHexGrid           Grid;
        private IUnitPositionCanon UnitPositionCanon;
        private IPlayerConfig      PlayerConfig;

        #endregion

        #region constructors

        [Inject]
        public BarbarianBrainTools(
            IHexGrid grid, IUnitPositionCanon unitPositionCanon, IPlayerConfig playerConfig
        ) {
            Grid              = grid;
            UnitPositionCanon = unitPositionCanon;
            PlayerConfig      = playerConfig;
        }

        #endregion

        #region instance methods

        public Func<IHexCell, int> GetWanderWeightFunction(IUnit unit, BarbarianInfluenceMaps maps) {
            var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

            return delegate(IHexCell cell) {
                if(cell == unitLocation || !UnitPositionCanon.CanPlaceUnitAtLocation(unit, cell, false)) {
                    return 0;
                }else {
                    float fromDistance = Grid.GetDistance(cell, unitLocation) * PlayerConfig.WanderSelectionWeight_Distance;
                    float fromAllies   = -maps.AllyPresence[cell.Index]       * PlayerConfig.WanderSelectionWeight_Allies;

                    return Math.Max(0, Mathf.RoundToInt(fromDistance + fromAllies));
                }
            };
        }

        #endregion

    }

}
