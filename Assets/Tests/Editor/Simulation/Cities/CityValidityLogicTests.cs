using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Cities;
using Assets.Simulation;
using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.Cities {

    [TestFixture]
    public class CityValidityLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IPossessionRelationship<ICity, IHexCell>> MockPossessionCanon;
        private Mock<IHexGrid>                                 MockHexGrid;
        private Mock<ICityFactory>                             MockCityFactory;
        private Mock<ICityConfig>                              MockConfig;
        private Mock<IRiverCanon>                              MockRiverCanon;
        private Mock<IPossessionRelationship<IHexCell, ICity>> MockCityLocationCanon;

        private List<ICity> AllCities = new List<ICity>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCities.Clear();

            MockPossessionCanon   = new Mock<IPossessionRelationship<ICity, IHexCell>>();
            MockHexGrid           = new Mock<IHexGrid>();
            MockCityFactory       = new Mock<ICityFactory>();
            MockConfig            = new Mock<ICityConfig>();
            MockRiverCanon        = new Mock<IRiverCanon>();
            MockCityLocationCanon = new Mock<IPossessionRelationship<IHexCell, ICity>>();

            MockCityFactory.Setup(factory => factory.AllCities).Returns(AllCities.AsReadOnly());

            Container.Bind<IPossessionRelationship<ICity, IHexCell>>().FromInstance(MockPossessionCanon  .Object);
            Container.Bind<IHexGrid>                                ().FromInstance(MockHexGrid          .Object);
            Container.Bind<ICityFactory>                            ().FromInstance(MockCityFactory      .Object);
            Container.Bind<ICityConfig>                             ().FromInstance(MockConfig           .Object);
            Container.Bind<IRiverCanon>                             ().FromInstance(MockRiverCanon       .Object);
            Container.Bind<IPossessionRelationship<IHexCell, ICity>>().FromInstance(MockCityLocationCanon.Object);

            Container.Bind<CityValidityLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "IsCellValidForCity should return false if the " +
            "argued cell already belongs to another city")]
        public void IsCellValidForCity_FalseIfTileBelongsToCity() {
            var cell = BuildCell(new Mock<ICity>().Object);

            var validityLogic = Container.Resolve<CityValidityLogic>();

            Assert.IsFalse(validityLogic.IsCellValidForCity(cell),
                "IsCellValidForCity returned true on a cell that's already owned");
        }

        [Test(Description = "IsCellValidForCity should return false if the " +
            "distance between this cell and any city is less than Config.MinSeparation")]
        public void IsCellValidForCity_FalseIfTileTooCloseToCity() {
            var cell = BuildCell(null);

            var firstLocation  = BuildCell(null);
            var secondLocation = BuildCell(null);
            var thirdlLocation = BuildCell(null);

            BuildCity(firstLocation);
            BuildCity(secondLocation);
            BuildCity(thirdlLocation);

            MockConfig.Setup(config => config.MinimumSeparation).Returns(3);

            MockHexGrid.Setup(grid => grid.GetDistance(cell, firstLocation)).Returns(2);
            MockHexGrid.Setup(grid => grid.GetDistance(cell, secondLocation)).Returns(3);
            MockHexGrid.Setup(grid => grid.GetDistance(cell, thirdlLocation)).Returns(4);

            var validityLogic = Container.Resolve<CityValidityLogic>();

            Assert.IsFalse(validityLogic.IsCellValidForCity(cell),
                "IsCellValidForCity returns true for a cell too close to existing cities");
        }

        [Test(Description = "IsCellValidForCity should return true if the " +
            "cell is unowned and no city is less than Config.MinSeparation away from it")]
        public void IsTileValidForCity_TrueOtherwise() {
            var cell = BuildCell(null);

            var firstLocation  = BuildCell(null);
            var secondLocation = BuildCell(null);

            BuildCity(firstLocation);
            BuildCity(secondLocation);

            MockConfig.Setup(config => config.MinimumSeparation).Returns(3);

            MockHexGrid.Setup(grid => grid.GetDistance(cell, firstLocation)).Returns(3);
            MockHexGrid.Setup(grid => grid.GetDistance(cell, secondLocation)).Returns(4);

            var validityLogic = Container.Resolve<CityValidityLogic>();

            Assert.IsTrue(validityLogic.IsCellValidForCity(cell),
                "IsCellValidForCity returns false for a cell that should be valid");
        }

        [Test(Description = "IsCellValidForCity should return false on any cell that " +
            "has a river or is underwater")]
        public void IsCellValidForCity_ConsidersRiversAndWater() {
            var underwaterCell        = BuildCell(null, true, false);
            var riveredCell           = BuildCell(null, false, true);
            var underwaterRiveredCell = BuildCell(null, true, true);

            var validityLogic = Container.Resolve<CityValidityLogic>();

            Assert.IsFalse(validityLogic.IsCellValidForCity(underwaterCell),
                "An underwater cell is incorrectly considered valid for city placement");

            Assert.IsFalse(validityLogic.IsCellValidForCity(riveredCell),
                "A rivered cell is incorrectly considered valid for city placement");

            Assert.IsFalse(validityLogic.IsCellValidForCity(underwaterRiveredCell),
                "An underwater rivered cell is incorrectly considered valid for city placement");
        }

        #endregion

        #region utilities

        private IHexCell BuildCell(ICity owner, bool isUnderwater = false, bool hasRiver = false) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.IsUnderwater).Returns(isUnderwater);

            MockPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(mockCell.Object)).Returns(owner);

            MockRiverCanon.Setup(canon => canon.HasRiver(mockCell.Object)).Returns(hasRiver);

            return mockCell.Object;
        }

        private ICity BuildCity(IHexCell location) {
            var mockCity = new Mock<ICity>();

            var newCity = mockCity.Object;

            MockCityLocationCanon.Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(location);
            AllCities.Add(newCity);

            return newCity;
        }

        #endregion

        #endregion

    }

}
