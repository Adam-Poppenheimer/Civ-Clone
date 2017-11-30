using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Core;

using Assets.Simulation.Cities;

namespace Assets.Tests.Simulation.Core {

    [TestFixture]
    public class GameCoreTests : ZenjectUnitTestFixture {

        private Mock<ITurnExecuter> TurnExecuterMock;
        private Mock<IRecordkeepingCityFactory> CityFactoryMock;

        [SetUp]
        public void CommonInstall() {
            TurnExecuterMock = new Mock<ITurnExecuter>();
            CityFactoryMock = new Mock<IRecordkeepingCityFactory>();

            Container.Bind<ITurnExecuter>().FromInstance(TurnExecuterMock.Object);
            Container.Bind<IRecordkeepingCityFactory>().FromInstance(CityFactoryMock.Object);

            Container.Bind<GameCore>().AsSingle();
        }

        [Test(Description = "When BeginRound is called, all cities in CityFactory have their " +
            "turns begun in TurnExecuter")]
        public void BeginRound_AllCitiesCalledToBeginTurn() {
            var allCities = new List<ICity>() {
                new Mock<ICity>().Object,
                new Mock<ICity>().Object,
                new Mock<ICity>().Object,
                new Mock<ICity>().Object,
                new Mock<ICity>().Object
            };

            CityFactoryMock.SetupGet(factory => factory.AllCities).Returns(allCities.AsReadOnly());

            var gameCore = Container.Resolve<GameCore>();

            gameCore.BeginRound();

            foreach(var city in allCities) {
                TurnExecuterMock.Verify(executer => executer.BeginTurnOnCity(city),
                    "TurnExecuter was not called to begin a city's turn");
            }
        }

        [Test(Description = "")]
        public void BeginRound_AllCivilizationsCalled() {
            throw new NotImplementedException();
        }

        [Test(Description = "")]
        public void BeginRound_HasCorrectExecutionOrder() {
            throw new NotImplementedException();
        }

        [Test(Description = "When EndRound is called, all cities in CityFactory have their " +
            "turns ended in TurnExecuter")]
        public void EndRound_AllCitiesCalledToEndTurn() {
            var allCities = new List<ICity>() {
                new Mock<ICity>().Object,
                new Mock<ICity>().Object,
                new Mock<ICity>().Object,
                new Mock<ICity>().Object,
                new Mock<ICity>().Object
            };

            CityFactoryMock.SetupGet(factory => factory.AllCities).Returns(allCities.AsReadOnly());

            var gameCore = Container.Resolve<GameCore>();

            gameCore.EndRound();

            foreach(var city in allCities) {
                TurnExecuterMock.Verify(executer => executer.EndTurnOnCity(city),
                    "TurnExecuter was not called to end a city's turn");
            }
        }

        [Test(Description = "")]
        public void EndRound_AllCivilizationsCalled() {
            throw new NotImplementedException();
        }

        [Test(Description = "")]
        public void EndRound_HasCorrectExecutionOrder() {
            throw new NotImplementedException();
        }

    }

}
