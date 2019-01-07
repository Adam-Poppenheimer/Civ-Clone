﻿using System;
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

namespace Assets.Simulation.Players.Barbarians {

    public class BarbarianPlayerBrain : IPlayerBrain {

        #region instance fields and properties

        #region from IPlayerBrain

        public string Name {
            get { return "Barbarian Brain"; }
        }

        #endregion

        private BarbarianInfluenceMaps LastMaps;




        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private ICivilizationFactory                          CivFactory;
        private IBarbarianUnitBrain                           BarbarianUnitBrain;
        private IUnitCommandExecuter                          UnitCommandExecuter;
        private IBarbarianInfluenceMapGenerator               InfluenceMapGenerator;

        #endregion

        #region constructors

        [Inject]
        public BarbarianPlayerBrain(
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            ICivilizationFactory civFactory, IBarbarianUnitBrain barbarianUnitBrain,
            IUnitCommandExecuter unitCommandExecuter, IBarbarianInfluenceMapGenerator influenceMapGenerator
        ) {
            UnitPossessionCanon   = unitPossessionCanon;
            CivFactory            = civFactory;
            BarbarianUnitBrain    = barbarianUnitBrain;
            UnitCommandExecuter   = unitCommandExecuter;
            InfluenceMapGenerator = influenceMapGenerator;
        }

        #endregion

        #region instance methods

        #region from IPlayerBrain

        public void RefreshAnalysis() {
            LastMaps = InfluenceMapGenerator.GenerateMaps();
        }

        public void ExecuteTurn(Action controlRelinquisher) {
            var barbarianCiv = CivFactory.AllCivilizations.FirstOrDefault(civ => civ.Template.IsBarbaric);

            if(barbarianCiv == null) {
                return;
            }

            foreach(var unit in UnitPossessionCanon.GetPossessionsOfOwner(barbarianCiv)) {
                var commands = BarbarianUnitBrain.GetCommandsForUnit(unit, LastMaps);

                UnitCommandExecuter.ClearCommandsForUnit(unit);
                UnitCommandExecuter.SetCommandsForUnit(unit, commands);
            }

            UnitCommandExecuter.IterateAllCommands(controlRelinquisher);
        }

        public void Clear() {
            InfluenceMapGenerator.ClearMaps();
        }

        #endregion

        #endregion

    }

}
