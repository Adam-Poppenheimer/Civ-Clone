using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.Civilizations {

    [TestFixture]
    public class CivilizationConnectionLogicTests : ZenjectUnitTestFixture {

        #region internal types

        public class AreCivilizationsConnectedTestData {

            public CivilizationTestData CivOne;
            public CivilizationTestData CivTwo;

            public bool PathExists;

        }

        public class CivilizationTestData {

            

        }

        #endregion

        #region static fields and properties

        public static IEnumerable AreCivilizationsConnectedTestCases {
            get {
                yield return new TestCaseData(new AreCivilizationsConnectedTestData() {
                    PathExists = true
                }).SetName("HexGrid returns non-null path between capitals").Returns(true);

                yield return new TestCaseData(new AreCivilizationsConnectedTestData() {
                    PathExists = false
                }).SetName("HexGrid returns null path between capitals").Returns(false);
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IHexPathfinder>                           MockHexPathfinder;
        private Mock<IPossessionRelationship<IHexCell, ICity>> MockCityLocationCanon;
        private Mock<ICapitalCityCanon>                        MockCapitalCityCanon;
        private Mock<IConnectionPathCostLogic>                 MockConnectionPathCostLogic;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockHexPathfinder           = new Mock<IHexPathfinder>();
            MockCityLocationCanon       = new Mock<IPossessionRelationship<IHexCell, ICity>>();
            MockCapitalCityCanon        = new Mock<ICapitalCityCanon>();
            MockConnectionPathCostLogic = new Mock<IConnectionPathCostLogic>();

            Container.Bind<IHexPathfinder>                          ().FromInstance(MockHexPathfinder          .Object);
            Container.Bind<IPossessionRelationship<IHexCell, ICity>>().FromInstance(MockCityLocationCanon      .Object);
            Container.Bind<ICapitalCityCanon>                       ().FromInstance(MockCapitalCityCanon       .Object);
            Container.Bind<IConnectionPathCostLogic>                ().FromInstance(MockConnectionPathCostLogic.Object);

            Container.Bind<CivilizationConnectionLogic>().AsSingle();
        }

        #endregion

        #region tests

        [TestCaseSource("AreCivilizationsConnectedTestCases")]
        [Test(Description = "")]
        public bool AreCivilizationsConnectedTests(AreCivilizationsConnectedTestData testData) {
            var civOneCapitalLocation = BuildHexCell();
            var civTwoCapitalLocation = BuildHexCell();

            var civOneCapital = BuildCity(civOneCapitalLocation);
            var civTwoCapital = BuildCity(civTwoCapitalLocation);

            var civOne = BuildCivilization(testData.CivOne, civOneCapital);
            var civTwo = BuildCivilization(testData.CivTwo, civTwoCapital);

            MockHexPathfinder
                .Setup(grid => grid.GetShortestPathBetween(
                    civOneCapitalLocation, civTwoCapitalLocation, It.IsAny<Func<IHexCell, IHexCell, float>>()
                )).Returns(testData.PathExists ? new List<IHexCell>() : null);

            MockHexPathfinder
                .Setup(grid => grid.GetShortestPathBetween(
                    civTwoCapitalLocation, civOneCapitalLocation, It.IsAny<Func<IHexCell, IHexCell, float>>()
                )).Returns(testData.PathExists ? new List<IHexCell>() : null);

            var connectionLogic = Container.Resolve<CivilizationConnectionLogic>();

            var oneConnectedToTwo = connectionLogic.AreCivilizationsConnected(civOne, civTwo);

            MockConnectionPathCostLogic.Verify(
                logic => logic.BuildPathCostFunction(civOne, civTwo), Times.Once,
                "ConnectionPathCostLogic.BuildPathCostFunction was not called as expected"
            );

            var twoConnectedToOne = connectionLogic.AreCivilizationsConnected(civTwo, civOne);

            Assert.AreEqual(
                oneConnectedToTwo, twoConnectedToOne,
                "AreCiviliationsConnected didn't produce the same result when its arguments were swapped"
            );

            return oneConnectedToTwo;
        }

        #endregion

        #region utilities

        private ICivilization BuildCivilization(CivilizationTestData civData, ICity capital) {
            var newCiv = new Mock<ICivilization>().Object;

            MockCapitalCityCanon.Setup(canon => canon.GetCapitalOfCiv(newCiv)).Returns(capital);

            return newCiv;
        }

        private IHexCell BuildHexCell() {
            return new Mock<IHexCell>().Object;
        }

        private ICity BuildCity(IHexCell location) {
            var newCity = new Mock<ICity>().Object;

            MockCityLocationCanon.Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(location);

            return newCity;
        }

        #endregion

        #endregion

    }


}
