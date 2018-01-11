using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Territory;
using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.Cities {

    [TestFixture]
    public class CityValidityLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ITilePossessionCanon> MockPossessionCanon;

        private Mock<IHexGrid> MockHexGrid;

        private Mock<ICityFactory> MockCityFactory;

        private Mock<ICityConfig> MockConfig;

        private Mock<IRiverCanon> MockRiverCanon;

        private List<ICity> AllCities = new List<ICity>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCities.Clear();

            MockPossessionCanon = new Mock<ITilePossessionCanon>();
            MockHexGrid         = new Mock<IHexGrid>();
            MockCityFactory     = new Mock<ICityFactory>();
            MockConfig          = new Mock<ICityConfig>();
            MockRiverCanon      = new Mock<IRiverCanon>();

            MockCityFactory.Setup(factory => factory.AllCities).Returns(AllCities.AsReadOnly());

            Container.Bind<ITilePossessionCanon>().FromInstance(MockPossessionCanon.Object);
            Container.Bind<IHexGrid>()            .FromInstance(MockHexGrid.Object);
            Container.Bind<ICityFactory>()        .FromInstance(MockCityFactory.Object);
            Container.Bind<ICityConfig>()         .FromInstance(MockConfig.Object);
            Container.Bind<IRiverCanon>()         .FromInstance(MockRiverCanon.Object);

            Container.Bind<CityValidityLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "IsTileValidForCity should return false if the " +
            "argued tile already belongs to another city")]
        public void IsTileValidForCity_FalseIfTileBelongsToCity() {
            var tile = BuildCell(new Mock<ICity>().Object);

            var validityLogic = Container.Resolve<CityValidityLogic>();

            Assert.IsFalse(validityLogic.IsCellValidForCity(tile),
                "IsTileValidForCity returned true on a tile that's already owned");
        }

        [Test(Description = "IsTileValidForCity should return false if the " +
            "distance between this tile and any city is less than Config.MinSeparation")]
        public void IsTileValidForCity_FalseIfTileTooCloseToCity() {
            var tile = BuildCell(null);

            var firstCity  = BuildCity(BuildCell(null));
            var secondCity = BuildCity(BuildCell(null));
            var thirdCity  = BuildCity(BuildCell(null));

            MockConfig.Setup(config => config.MinimumSeparation).Returns(3);

            MockHexGrid.Setup(grid => grid.GetDistance(tile, firstCity .Location)).Returns(2);
            MockHexGrid.Setup(grid => grid.GetDistance(tile, secondCity.Location)).Returns(3);
            MockHexGrid.Setup(grid => grid.GetDistance(tile, thirdCity .Location)).Returns(4);

            var validityLogic = Container.Resolve<CityValidityLogic>();

            Assert.IsFalse(validityLogic.IsCellValidForCity(tile),
                "IsTileValidForCity returns true for a tile too close to existing cities");
        }

        [Test(Description = "IsTileValidForCity should return true if the " +
            "tile is unowned and no city is less than Config.MinSeparation away from it")]
        public void IsTileValidForCity_TrueOtherwise() {
            var tile = BuildCell(null);

            var firstCity  = BuildCity(BuildCell(null));
            var secondCity = BuildCity(BuildCell(null));

            MockConfig.Setup(config => config.MinimumSeparation).Returns(3);

            MockHexGrid.Setup(grid => grid.GetDistance(tile, firstCity .Location)).Returns(3);
            MockHexGrid.Setup(grid => grid.GetDistance(tile, secondCity.Location)).Returns(4);

            var validityLogic = Container.Resolve<CityValidityLogic>();

            Assert.IsTrue(validityLogic.IsCellValidForCity(tile),
                "IsTileValidForCity returns false for a tile that should be valid");
        }

        [Test(Description = "IsCellValidForCity should return false on any tile that " +
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

            MockPossessionCanon.Setup(canon => canon.GetCityOfTile(mockCell.Object)).Returns(owner);

            MockRiverCanon.Setup(canon => canon.HasRiver(mockCell.Object)).Returns(hasRiver);

            return mockCell.Object;
        }

        private ICity BuildCity(IHexCell location) {
            var mockCity = new Mock<ICity>();

            mockCity.Setup(city => city.Location).Returns(location);

            AllCities.Add(mockCity.Object);
            return mockCity.Object;
        }

        #endregion

        #endregion

    }

}
