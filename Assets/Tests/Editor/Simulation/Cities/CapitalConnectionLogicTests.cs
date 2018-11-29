using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.Cities {

    public class CapitalConnectionLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ICapitalCityCanon>                             MockCapitalCityCanon;
        private Mock<IHexPathfinder>                                MockHexPathfinder;
        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;
        private Mock<IPossessionRelationship<IHexCell, ICity>>      MockCityLocationCanon;
        private Mock<IConnectionPathCostLogic>                      MockConnectionPathCostLogic;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockCapitalCityCanon        = new Mock<ICapitalCityCanon>();
            MockHexPathfinder           = new Mock<IHexPathfinder>();
            MockCityPossessionCanon     = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            MockCityLocationCanon       = new Mock<IPossessionRelationship<IHexCell, ICity>>();
            MockConnectionPathCostLogic = new Mock<IConnectionPathCostLogic>();

            Container.Bind<ICapitalCityCanon>                            ().FromInstance(MockCapitalCityCanon       .Object);
            Container.Bind<IHexPathfinder>                               ().FromInstance(MockHexPathfinder          .Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon    .Object);
            Container.Bind<IPossessionRelationship<IHexCell, ICity>>     ().FromInstance(MockCityLocationCanon      .Object);
            Container.Bind<IConnectionPathCostLogic>                     ().FromInstance(MockConnectionPathCostLogic.Object);

            Container.Bind<CapitalConnectionLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void IsCityConnectedToCapital_CallsIntoPathfinderAndPathCostLogicCorrectly() {
            var cityToTestLocation = BuildCell();
            var capitalLocation    = BuildCell();

            var cityToTest = BuildCity(cityToTestLocation);
            var capital    = BuildCity(capitalLocation);

            var owner = BuildCiv(capital, cityToTest);

            var connectionLogic = Container.Resolve<CapitalConnectionLogic>();

            Func<IHexCell, IHexCell, float> pathCostFunction = (a, b) => 1f;

            MockConnectionPathCostLogic.Setup(
                logic => logic.BuildCapitalConnectionPathCostFunction(owner)
            ).Returns(pathCostFunction);

            connectionLogic.IsCityConnectedToCapital(cityToTest);

            MockConnectionPathCostLogic.Verify(
                logic => logic.BuildCapitalConnectionPathCostFunction(owner),
                Times.Once, "Failed to call into ConnectionPathCostLogic correctly"
            );

            MockHexPathfinder.Verify(
                pathfinder => pathfinder.GetShortestPathBetween(cityToTestLocation, capitalLocation, pathCostFunction),
                Times.Once, "Failed to call into HexPathfinder correctly"
            );
        }

        [Test]
        public void IsCityConnectedToCapital_TrueIfCityIsCapital() {
            var capital = BuildCity(BuildCell());

            BuildCiv(capital);

            var connectionLogic = Container.Resolve<CapitalConnectionLogic>();

            Assert.IsTrue(connectionLogic.IsCityConnectedToCapital(capital));
        }

        [Test]
        public void IsCityConnectedToCapital_TrueIfSomePathExists() {
            var cityToTest = BuildCity(BuildCell());
            var capital    = BuildCity(BuildCell());

            BuildCiv(capital, cityToTest);

            var connectionLogic = Container.Resolve<CapitalConnectionLogic>();

            MockHexPathfinder.Setup(
                pathfinder => pathfinder.GetShortestPathBetween(
                    It.IsAny<IHexCell>(), It.IsAny<IHexCell>(),
                    It.IsAny<Func<IHexCell, IHexCell, float>>()
                )
            ).Returns(new List<IHexCell>());

            Assert.IsTrue(connectionLogic.IsCityConnectedToCapital(cityToTest));
        }

        [Test]
        public void IsCityConnectedToCapital_FalseIfReturnedPathIsNull() {
            var cityToTest = BuildCity(BuildCell());
            var capital    = BuildCity(BuildCell());

            BuildCiv(capital, cityToTest);

            var connectionLogic = Container.Resolve<CapitalConnectionLogic>();

            Assert.IsFalse(connectionLogic.IsCityConnectedToCapital(cityToTest));
        }

        #endregion

        #region utilities

        private IHexCell BuildCell() {
            return new Mock<IHexCell>().Object;
        }

        private ICity BuildCity(IHexCell location) {
            var newCity = new Mock<ICity>().Object;

            MockCityLocationCanon.Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(location);

            return newCity;
        }

        private ICivilization BuildCiv(ICity capital, params ICity[] otherCities) {
            var newCiv = new Mock<ICivilization>().Object;

            MockCapitalCityCanon.Setup(canon => canon.GetCapitalOfCiv(newCiv)).Returns(capital);

            MockCityPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCiv))
                                   .Returns(otherCities);

            foreach(var city in otherCities) {
                MockCityPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(city)).Returns(newCiv);
            }

            return newCiv;
        }

        #endregion

        #endregion

    }

}
