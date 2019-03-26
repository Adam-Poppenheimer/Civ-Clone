using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.Core;
using Assets.Simulation.Cities;

namespace Assets.Tests.Simulation.Core {

    public class CityRoundExecuterTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ICityFactory> MockCityFactory;

        private List<ICity> AllCities = new List<ICity>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCities.Clear();

            MockCityFactory = new Mock<ICityFactory>();

            MockCityFactory.Setup(factory => factory.AllCities).Returns(AllCities.AsReadOnly());

            Container.Bind<ICityFactory>().FromInstance(MockCityFactory.Object);

            Container.Bind<CityRoundExecuter>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void PerformStartOfRoundActions_EachCity_HasProperMethodsCalledInOrder() {
            var cityMocks = new List<Mock<ICity>>() {
                BuildMockCity(), BuildMockCity(), BuildMockCity()
            };

            List<MockSequence> mockSequences = new List<MockSequence>();

            foreach(var cityMock in cityMocks) {
                var sequence = new MockSequence();

                cityMock.InSequence(sequence).Setup(city => city.PerformProduction());
                cityMock.InSequence(sequence).Setup(city => city.PerformGrowth());
                cityMock.InSequence(sequence).Setup(city => city.PerformExpansion());
                cityMock.InSequence(sequence).Setup(city => city.PerformDistribution());

                mockSequences.Add(sequence);
            }

            var roundExecuter = Container.Resolve<CityRoundExecuter>();

            roundExecuter.PerformStartOfRoundActions();
            
            foreach(var cityMock in cityMocks) {
                cityMock.VerifyAll();
            }
        }

        [Test]
        public void PerformEndOfRoundActions_EachCity_HasProperMethodsCalledInOrder() {
            var cityMocks = new List<Mock<ICity>>() {
                BuildMockCity(), BuildMockCity(), BuildMockCity()
            };

            List<MockSequence> mockSequences = new List<MockSequence>();

            foreach(var cityMock in cityMocks) {
                var sequence = new MockSequence();

                cityMock.InSequence(sequence).Setup(city => city.PerformIncome());

                mockSequences.Add(sequence);
            }

            var roundExecuter = Container.Resolve<CityRoundExecuter>();

            roundExecuter.PerformEndOfRoundActions();
            
            foreach(var cityMock in cityMocks) {
                cityMock.VerifyAll();
            }
        }

        #endregion

        #region utilities

        private Mock<ICity> BuildMockCity() {
            var mockCity = new Mock<ICity>();

            AllCities.Add(mockCity.Object);

            return mockCity;
        }

        #endregion

        #endregion

    }

}
