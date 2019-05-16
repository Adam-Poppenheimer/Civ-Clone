using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.Core;
using Assets.Simulation.Diplomacy;

namespace Assets.Tests.Simulation.Core {

    public class RoundExecutionSequencerTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IDiplomacyCore> MockDiplomacyCore;
        private CoreSignals          CoreSignals;

        private List<IRoundExecuter> RoundExecuters = new List<IRoundExecuter>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            RoundExecuters.Clear();

            MockDiplomacyCore = new Mock<IDiplomacyCore>();
            CoreSignals       = new CoreSignals();

            Container.Bind<IDiplomacyCore>      ().FromInstance(MockDiplomacyCore.Object);
            Container.Bind<CoreSignals>         ().FromInstance(CoreSignals);
            Container.Bind<List<IRoundExecuter>>().FromInstance(RoundExecuters);

            Container.Bind<RoundExecutionSequencer>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void OnStartingRoundFired_PerformsStartOfRoundActionsForAllExecuters() {
            var mockExecuters = new List<Mock<IRoundExecuter>>() {
                BuildMockRoundExecuter(), BuildMockRoundExecuter(), BuildMockRoundExecuter()
            };

            Container.Resolve<RoundExecutionSequencer>();

            CoreSignals.StartingRound.OnNext(-1);

            foreach(var mockExecuter in mockExecuters) {
                mockExecuter.Verify(
                    executer => executer.PerformStartOfRoundActions(), Times.Once,
                    "An executer did not have its PerformStartOfRoundActions method called"
                );
            }
        }

        [Test]
        public void OnEndingRoundFired_PerformsEndOfRoundActionsForAllExecuters() {
            var mockExecuters = new List<Mock<IRoundExecuter>>() {
                BuildMockRoundExecuter(), BuildMockRoundExecuter(), BuildMockRoundExecuter()
            };

            Container.Resolve<RoundExecutionSequencer>();

            CoreSignals.EndingRound.OnNext(-1);

            foreach(var mockExecuter in mockExecuters) {
                mockExecuter.Verify(
                    executer => executer.PerformEndOfRoundActions(), Times.Once,
                    "An executer did not have its PerformEndOfRoundActions method called"
                );
            }
        }

        [Test]
        public void OnEndingRoundFired_UpdatesOngoingDeals() {
            Container.Resolve<RoundExecutionSequencer>();

            CoreSignals.EndingRound.OnNext(-1);

            MockDiplomacyCore.Verify(core => core.UpdateOngoingDeals(), Times.Once);
        }

        #endregion

        private Mock<IRoundExecuter> BuildMockRoundExecuter() {
            var mockExecuter = new Mock<IRoundExecuter>();

            RoundExecuters.Add(mockExecuter.Object);

            return mockExecuter;
        }

        #endregion

    }

}
