using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.Units;
using Assets.Simulation.Civilizations;
using Assets.Simulation.AI;
using Assets.Simulation.Players;

namespace Assets.Simulation.Barbarians {

    public class BarbarianPlayerBrain : IPlayerBrain {

        #region instance fields and properties

        #region from IPlayerBrain

        public string Name {
            get { return "Barbarian Brain"; }
        }

        #endregion

        private InfluenceMaps LastMaps = new InfluenceMaps();




        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private IBarbarianUnitBrain                           BarbarianUnitBrain;
        private IUnitCommandExecuter                          UnitCommandExecuter;
        private IBarbarianTurnExecuter                        TurnExecuter;
        private IInfluenceMapLogic                            InfluenceMapLogic;

        #endregion

        #region constructors

        [Inject]
        public BarbarianPlayerBrain(
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            IBarbarianUnitBrain barbarianUnitBrain, IUnitCommandExecuter unitCommandExecuter,
            IBarbarianTurnExecuter turnExecuter, IInfluenceMapLogic influenceMapLogic
        ) {
            UnitPossessionCanon = unitPossessionCanon;
            BarbarianUnitBrain  = barbarianUnitBrain;
            UnitCommandExecuter = unitCommandExecuter;
            TurnExecuter        = turnExecuter;
            InfluenceMapLogic   = influenceMapLogic;
        }

        #endregion

        #region instance methods

        #region from IPlayerBrain

        public void RefreshAnalysis(IPlayer activePlayer) {
            InfluenceMapLogic.AssignMaps(LastMaps, activePlayer.ControlledCiv);
        }

        public void ExecuteTurn(IPlayer activePlayer, Action controlRelinquisher) {
            TurnExecuter.PerformEncampmentSpawning(LastMaps);
            TurnExecuter.PerformUnitSpawning();

            foreach(var unit in UnitPossessionCanon.GetPossessionsOfOwner(activePlayer.ControlledCiv)) {
                var commands = BarbarianUnitBrain.GetCommandsForUnit(unit, LastMaps);

                UnitCommandExecuter.ClearCommandsForUnit(unit);
                UnitCommandExecuter.SetCommandsForUnit(unit, commands);
            }

            UnitCommandExecuter.IterateAllCommands(controlRelinquisher);
        }

        public void Clear() {
            InfluenceMapLogic.ClearMaps(LastMaps);
        }

        #endregion

        #endregion

    }

}
