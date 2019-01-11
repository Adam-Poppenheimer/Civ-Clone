﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.Players;
using Assets.Simulation.Civilizations;

namespace Assets.Tests.Simulation.Players {

    public class PlayerTests : ZenjectUnitTestFixture {

        #region instance methods

        #region tests

        [Test]
        public void PassControl_CorrectBrainMethodsCalledInOrder() {
            var civ = new Mock<ICivilization>().Object;

            var mockBrain = new Mock<IPlayerBrain>(MockBehavior.Strict);

            var executionSequence = new MockSequence();

            mockBrain.InSequence(executionSequence).Setup(brain => brain.RefreshAnalysis());
            mockBrain.InSequence(executionSequence).Setup(brain => brain.ExecuteTurn(It.IsAny<Action>()));

            Action controlRelinquisher = () => { };

            var player = new Player(civ, mockBrain.Object);

            player.PassControl(controlRelinquisher);

            mockBrain.VerifyAll();

            mockBrain.Verify(
                brain => brain.ExecuteTurn(controlRelinquisher),
                Times.Once, "ControlRelinquisher not passed as expected"
            );
        }

        [Test]
        public void Clear_ClearCalledOnBrain() {
            var civ = new Mock<ICivilization>().Object;

            var mockBrain = new Mock<IPlayerBrain>();

            var player = new Player(civ, mockBrain.Object);

            player.Clear();

            mockBrain.Verify(brain => brain.Clear(), Times.Once);
        }

        #endregion

        #endregion

    }

}